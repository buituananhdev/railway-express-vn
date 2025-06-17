import http from "k6/http";
import { check } from "k6";

export let options = {
    vus: 100,
    duration: "60s",
};

export default function () {
    const url = "https://railway-api-gateway.happysky-b2c92079.southeastasia.azurecontainerapps.io/admin/v1/seats?trainCarId=08dda1a6-c462-4a23-8ee2-9a7746f5f440&trainScheduleId=08dda1a6-c807-4d4e-84b1-0c375de9783a&journeyDate=2025-06-07T14:52:30";

    const res = http.get(url);

    check(res, {
        "status was 200": (r) => r.status === 200,
    });
}
