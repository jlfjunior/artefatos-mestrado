import http from "k6/http";
import { check, fail } from "k6";

const baseUrl = __ENV.API_BASE_URL || "http://localhost:8081";
const clientUserId = __ENV.USER_ID || "40830623-48b3-413f-9319-375501484841";
const balanceDate = __ENV.BALANCE_DATE;
const writeRate = Number(__ENV.WRITE_RATE || 15);
const readRate = Number(__ENV.READ_RATE || 50);
const duration = __ENV.DURATION || "30s";

export const options = {
  scenarios: {
    transaction_writes: {
      executor: "constant-arrival-rate",
      rate: writeRate,
      timeUnit: "1s",
      duration,
      preAllocatedVUs: Math.max(20, writeRate),
      maxVUs: Math.max(50, writeRate * 2),
      exec: "writeTransaction",
    },
    balance_reads: {
      executor: "constant-arrival-rate",
      rate: readRate,
      timeUnit: "1s",
      duration,
      preAllocatedVUs: Math.max(50, readRate),
      maxVUs: Math.max(100, readRate * 2),
      exec: "readBalance",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.05"],
    "http_req_duration{scenario:balance_reads}": ["p(95)<500"],
    "http_req_duration{scenario:transaction_writes}": ["p(95)<1000"],
  },
};

function authHeaders(token) {
  return {
    Authorization: `Bearer ${token}`,
    "Content-Type": "application/json",
  };
}

function resolveAccessToken(response) {
  const body = response.json();
  return body.access_token || null;
}

function acquireToken() {
  const payload = {
    grant_type: "password",
    client_id: "cashflow.web",
    client_secret: "bb222c98-ead0-44cd-b12a-a54a9f6ee1a4",
    username: "funcionario@cashflow.local",
    password: "Funcionario@123",
  };

  const response = http.post(`${baseUrl}/oauth/token`, payload, {
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
  });

  const accessToken = resolveAccessToken(response);

  check(response, {
    "token acquired": (item) => item.status === 200 && accessToken !== null,
  });

  if (!accessToken) {
    fail(
      `Falha ao obter access token (HTTP ${response.status}): ${response.body}`,
    );
  }

  return accessToken;
}

function seedTransactions(token, count) {
  for (let i = 0; i < count; i += 1) {
    const body = JSON.stringify({
      user_id: clientUserId,
      description: `Seed load test ${i}`,
      amount: 10 + i,
      entry_type: i % 2 === 0 ? "Credit" : "Debit",
    });

    http.post(`${baseUrl}/api/transactions`, body, {
      headers: authHeaders(token),
    });
  }
}

export function setup() {
  const accessToken = acquireToken();
  seedTransactions(accessToken, 5);

  return { accessToken, clientUserId };
}

export function writeTransaction(data) {
  const entryType = __ITER % 2 === 0 ? "Credit" : "Debit";
  const body = JSON.stringify({
    user_id: data.clientUserId,
    description: `k6 write vu=${__VU} iter=${__ITER}`,
    amount: 1 + (__ITER % 500) + 0.01,
    entry_type: entryType,
  });

  const response = http.post(`${baseUrl}/api/transactions`, body, {
    headers: authHeaders(data.accessToken),
    tags: { operation: "write" },
  });

  check(response, {
    "write status is 201": (item) => item.status === 201,
  });
}

export function readBalance(data) {
  const path = balanceDate
    ? `/api/balances/${balanceDate}?userId=${data.clientUserId}`
    : `/api/balances/today?userId=${data.clientUserId}`;

  const response = http.get(`${baseUrl}${path}`, {
    headers: { Authorization: `Bearer ${data.accessToken}` },
    tags: { operation: "read" },
  });

  check(response, {
    "read status is 200": (item) => item.status === 200,
  });
}
