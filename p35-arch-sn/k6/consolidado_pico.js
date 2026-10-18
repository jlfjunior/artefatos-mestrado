import http from 'k6/http';
import { check, sleep } from 'k6';

// Meta: 50 req/s, menos de 5% erro, p95 < 200ms
export const options = {
  scenarios: {
    pico_consolidado: {
      executor: 'constant-arrival-rate',
      rate: 50,
      timeUnit: '1s',
      duration: '2m',
      preAllocatedVUs: 60,
      maxVUs: 100,
    },
  },
  thresholds: {
    http_req_failed:   ['rate<0.05'], 
    http_req_duration: ['p(95)<200'], 
    http_req_duration: ['p(99)<500'], 
  },
};

export default function () {
  const data = '2026-03-30';
  const url = `http://localhost:5000/api/consolidado/${data}`;

  const res = http.get(url, {
    headers: { 
      'Content-Type': 'application/json'
    },
  });

  check(res, {
    'status 200': (r) => r.status === 200,
  });
}
