import http from "k6/http";

export const options = {
  scenarios: {
    rate_limit: {
      executor: "constant-arrival-rate",
      rate: 300,
      timeUnit: "60s",
      duration: "3m",
      preAllocatedVUs: 20
    }
  }
};

export default function() {
  const getRequiredEnvVar = name => {
    const value = __ENV[name];

    if (!value) {
      throw new Error(`Missing environment variable: '${name}'.`);
    }

    return value;
  };

  const apiKey = getRequiredEnvVar('API_KEY');
  const host = getRequiredEnvVar('HOST');
  const trn = getRequiredEnvVar('TRN');
  const birthDate = getRequiredEnvVar('BIRTH_DATE');

  const params = {
    headers: {
      "Authorization": `Bearer ${apiKey}`
    }
  };

  http.get(`${host}/v1/teachers/${trn}?birthdate=${birthDate}`, params);
}
