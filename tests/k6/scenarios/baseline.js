// Baseline load test: authenticated, read-heavy API traffic of the customer portal.
//   docker run --rm -e BASE_URL=http://host.docker.internal:8080 -e VUS=10 -v "$PWD/tests/k6:/scripts" grafana/k6 run /scripts/scenarios/baseline.js
// VUS and DURATION make the same script reusable for the 10/50/100/250/500 user profiles.
import http from "k6/http";
import { check, fail } from "k6";

const BASE_URL = __ENV.BASE_URL || "http://localhost:8080";
const EMAIL = __ENV.EMAIL || "admin@acme.demo";
const PASSWORD = __ENV.PASSWORD || "DataForge!Demo2026";

export const options = {
  scenarios: {
    baseline: {
      executor: "constant-vus",
      vus: Number(__ENV.VUS || 10),
      duration: __ENV.DURATION || "1m",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    "http_req_duration{endpoint:projects}": ["p(95)<300"],
    "http_req_duration{endpoint:jobs}": ["p(95)<300"],
    "http_req_duration{endpoint:me}": ["p(95)<150"],
  },
  summaryTrendStats: ["avg", "min", "med", "p(90)", "p(95)", "p(99)", "max"],
};

// Signs in once and shares the session cookie with every virtual user (login is rate limited per IP).
export function setup() {
  const antiforgery = http.get(`${BASE_URL}/api/auth/antiforgery`);
  const { token, headerName } = antiforgery.json("data");
  const login = http.post(
    `${BASE_URL}/api/auth/login`,
    JSON.stringify({ email: EMAIL, password: PASSWORD }),
    { headers: { "Content-Type": "application/json", [headerName]: token } },
  );
  if (login.status !== 200) {
    fail(`login failed with status ${login.status}`);
  }
  return { session: login.cookies["dataforge.auth"][0].value };
}

export default function (data) {
  const params = (endpoint) => ({ headers: { Cookie: `dataforge.auth=${data.session}` }, tags: { endpoint } });

  const projects = http.get(`${BASE_URL}/api/projects`, params("projects"));
  check(projects, { "projects 200": (r) => r.status === 200 });

  const jobs = http.get(`${BASE_URL}/api/jobs?limit=20`, params("jobs"));
  check(jobs, { "jobs 200": (r) => r.status === 200 });

  const me = http.get(`${BASE_URL}/api/auth/me`, params("me"));
  check(me, { "me 200": (r) => r.status === 200 });
}
