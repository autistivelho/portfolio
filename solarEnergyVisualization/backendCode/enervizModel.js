const request = require("request");
const moment = require("moment");

const clearJson = require("./clearJson");

const url = "https://api.foremica.fi/v1/";
const apiKey = "b1b7e382-d01f-46f7-86a3-e1867908ca4f";

module.exports = {
    "getData": (callback) => {
        let currentDate = moment();
        let currentTimestamp = currentDate.valueOf();
        let monthAgoTimestamp = currentDate.subtract(1, "months").valueOf();

        var options = {
            uri: url,
            qs: {
                apikey: apiKey,
                format: "json",
                from: monthAgoTimestamp,
                to: currentTimestamp
            },
            json: true
        }

        request(options, (err, data) => {
            clearJson.Parse(data.body, (cleanedData) => {
                callback(cleanedData);
            });
        });
    }
}