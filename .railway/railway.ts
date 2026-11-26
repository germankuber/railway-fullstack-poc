import { defineRailway, postgres, project, service, volume } from "railway/iac";

export default defineRailway(() => {
  const Postgres = postgres("Postgres", { region: "iad" });
  Postgres.deploy = { multiRegionConfig: { iad: { numReplicas: 1 } } };
  Postgres.networking = { privateNetworkEndpoint: "postgres" };
  const postgresVolume = volume("postgres-volume", { alerts: { usage: { "100": {}, "80": {}, "95": {} } }, allowOnlineResize: true, region: "iad", sizeMB: 500 });
  const web = service("web", {
    build: { buildEnvironment: "V3", builder: "RAILPACK" },
    deploy: { healthcheckPath: "/", healthcheckTimeout: 60 },
    replicas: { "iad": 1 },
  });
  const api = service("api", {
    build: { buildEnvironment: "V3", builder: "RAILPACK" },
    deploy: {
      preDeployCommand: ["dotnet TodoApi.dll --migrate"],
      preDeployTimeoutSeconds: 300,
      healthcheckPath: "/health",
      healthcheckTimeout: 60,
    },
    replicas: { "iad": 1 },
  });

  return project("railway-fullstack-poc", {
    variables: { managed: false },
    resources: [web, api, Postgres, postgresVolume],
  });
});
