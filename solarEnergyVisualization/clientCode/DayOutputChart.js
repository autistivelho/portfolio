import React, { Component } from "react";
import Chart from "react-apexcharts";
import "apexcharts/dist/locales/fi"

class DayOutputChart extends Component {
  constructor(props) {
    super(props);
    this.state = {};
  }

  render() {
    return (
      <div className="lineChart">
        <h2>Energiaa päivän kuluessa</h2>
        <Chart
          options={this.props.values.options}
          series={this.props.values.series}
          type="line"
          height="350"
          width="97%"
        />
      </div>
    );
  }
}

export default DayOutputChart;