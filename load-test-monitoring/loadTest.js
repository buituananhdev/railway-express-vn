import http from "k6/http";
import { check } from "k6";

export let options = {
    vus: 100,
    duration: "30s",
};

export default function () {
    const url =
        "https://railway-api-gateway.happysky-b2c92079.southeastasia.azurecontainerapps.io/admin/v1/trainschedules/get-schedules";

    const payload = JSON.stringify({
        departureStationId: "dd42fb58-251e-4a07-88c7-920195a91435",
        arrivalStationId: "745f5a08-0f09-4300-ac30-5bbed1457ca3",
        departureTime: "2025-06-02T13:15:49",
        returnTime: null,
    });

    const params = {
        headers: {
            "Content-Type": "application/json",
        },
    };

    const res = http.post(url, payload, params);

    check(res, {
        "status was 200": (r) => r.status === 200,
    });
}
