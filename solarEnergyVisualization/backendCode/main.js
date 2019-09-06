const express = require("express");
const app = express();
const bodyParser = require("body-parser");
const enervizModel = require("./models/enervizModel");

const port = "3019";

app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ "extended": true }));

app.use((req, res, next) => {
    res.header("Access-Control-Allow-Origin", "*");
    res.header("Access-Control-Allow-Methods", "GET");
    res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-type, Accept");
    next();
});

app.get("/", (req, res) => {

    enervizModel.getData((data) => {

        res.send(data);

    });

});

app.listen(port, () => {
    console.log(`enerviz REST API started, running on port ${port}.`);
});