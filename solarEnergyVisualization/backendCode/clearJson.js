exports.Parse = (jsonData, callback) => {
    let cleanedData = {
        values: []
    };

    jsonData.valueRows.forEach(item => {
        if(item.values[0] !== null) {
            cleanedData.values.push({
                timestamp: item.timestamp,
                lifecycleProduction: {
                    unit: jsonData.units[0],
                    value: item.values[0] > 0 ? item.values[0] : 0,
                },
                powerOutput: {
                    unit: jsonData.units[1],
                    value: item.values[1] > 0 ? item.values[1] : 0,
                }
            });
        }
    });

    cleanedData.values.sort((a, b) => {
        return a.timestamp - b.timestamp;
    });

    callback(cleanedData);
}