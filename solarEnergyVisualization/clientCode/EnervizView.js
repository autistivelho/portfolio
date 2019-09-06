import React, { Component } from 'react';
import * as moment from 'moment';
import Moment from 'react-moment';
import 'moment/locale/fi';
import fi from 'apexcharts/dist/locales/fi';
import CountUp from 'react-countup';
import '../styles/custom.scss';
import logo from '../data/images/logo.png';
import DayOutputChart from '../components/DayOutputChart';
import MonthOutputChart from '../components/MonthOutputChart';

class EnervizView extends Component {
  constructor(props) {
    super(props);
    this.fetchData();
  }

  componentDidMount() {
    this.intervalFetch = setInterval(() => this.fetchData(), 60000);
  }

  componentWillUnmount() {
    clearInterval(this.intervalFetch);
  }

  render() {
    if (!this.state) {
      return <div />
    }

    return (
      <div className="container-fluid p-0">
        <div className="jumbotron jumbotron-fluid px-5 py-5 d-flex justify-content-between header">
          <div className="header-spacer"></div>
          <h1 className="title-text">Aurinkopaneelien energiantuotanto</h1>
          <div><img src={logo} width="200" alt="" /></div>
        </div>
        <div className="d-flex justify-content-start px-3 pt-1 pb-3 h-50">
          <div className="statistics-card">
            <h2>Tämänhetkinen tuotto</h2>
            <h1 className="display-2 value-text">
              <CountUp end={Math.round(this.state.lastEntry.powerOutput.value)} duration={1}></CountUp>
              {" "}
              {this.state.lastEntry.powerOutput.unit}
            </h1>
            <p>
              päivitetty viimeksi <Moment unix fromNow>{this.state.lastEntry.timestamp / 1000}</Moment>
            </p>
          </div>
          <div className="statistics-card">
            <h2>Viimeisen kuukauden tuotto</h2>
            <h1 className="display-2 value-text">
              <CountUp end={Math.round(this.state.monthTotalProduction)} duration={1}></CountUp>
              {" "}
              {this.state.lastEntry.lifecycleProduction.unit}
            </h1>
            <p>
              päivitetty viimeksi <Moment unix fromNow>{this.state.lastEntry.timestamp / 1000}</Moment>
            </p>
          </div>
          <div className="statistics-card">
            <h2>Asennuksesta lähtien tuotettu</h2>
            <h1 className="display-2 value-text">
              <CountUp end={Math.round(this.state.lastEntry.lifecycleProduction.value)} duration={1}></CountUp>
              {" "}
              {this.state.lastEntry.lifecycleProduction.unit}
            </h1>
            <p>
              päivitetty viimeksi <Moment unix fromNow>{this.state.lastEntry.timestamp / 1000}</Moment>
            </p>
          </div>
        </div>
        <div className="d-flex justify-content-start px-3 py-3">
          <div className="flex-fill line-chart-card w-50">
            <DayOutputChart values={this.state.dayOutputChart}></DayOutputChart>
          </div>
          <div className="flex-fill line-chart-card w-50">
            <MonthOutputChart values={this.state.monthOutputChart}></MonthOutputChart>
          </div>
        </div>
      </div>
    );
  }

  fetchData() {
    fetch("http://localhost:3019/")
      .then((res) => res.json())
      .then((data) => {
        let stateObject = this.mapData(data);
        this.setState(stateObject);
      })
      .catch((err) => {
        console.log("problems when fetching data. Error: " + err)
      });
  }

  mapData(data) {

    let powerOutputMonthValues = Array.from(data.values, x => [x.timestamp, x.powerOutput.value]);

    let powerOutputMonthAverages = [];

    powerOutputMonthAverages.push([0, 0]);

    // asetetaan muistiin 24 tuntia aika-arvona sisältävä aikaleima.
    let fullDayTimestamp = 86400000;

    // Asetetaan valmiiksi jakaja-arvo, jota tarvitaan myöhemmin.
    let divider = 0;

    // Käydään läpi kaikki aika-energiamääräparit taulukkomuuttujasta powerOutputValues.
    // Tavoitteena rakentaa uusi taulukko powerOutputMonthAverages,
    // joka sisältää kuukauden ajalta jokaisen päivän keskiarvon tuotetusta energiasta watteina.
    powerOutputMonthValues.forEach((value, index, array) => {

      // Nostetaan jakajan arvoa jokaisen läpikäydyn objektin kohdalla,
      // jakajan arvo vastaa siis aina kuhunkin keskiarvoon laskettavien arvojen määrää
      divider++;

      // Jos nykyisen objektin aikaleima on jaollinen tasan 24 tunnilla,
      // eli se on kokonainen päivä, otetaan viimeisin objekti 
      // (eli tähän mennessä kaikkien mittaustulosten yhteenlaskettu arvo) 
      // uudesta taulukosta, asetetaan siihen aika-arvoksi tasapäivä sekä energiamääräksi 
      // keskiarvo edeltävän päivän energiamääristä,
      // asetetaan jakajan arvo takaisin nollaan ja
      // siirrytään seuraavaan objektiin.
      if (value[0] % fullDayTimestamp === 0) {
        let lastEntry = powerOutputMonthAverages[powerOutputMonthAverages.length - 1];
        let newEntry = [value[0], (lastEntry[1] + value[1]) / divider];
        powerOutputMonthAverages[powerOutputMonthAverages.length - 1] = newEntry;
        powerOutputMonthAverages.push([0, 0]);
        divider = 0;
        return;
      }

      // Otetaan viimeisin objekti uudesta taulukosta,
      // tehdään uusi muuttuja, jonka arvoiksi asetetaan 
      // käsiteltävän objektin aika-arvo ja lasketaan yhteen
      // viimeisimmän objektin energiamääräarvo ja 
      // käsiteltävän objektin vastaava arvo.
      let lastEntry = powerOutputMonthAverages[powerOutputMonthAverages.length - 1];
      let newEntry = [value[0], lastEntry[1] + value[1]];

      // jos käsiteltävä objekti on vanhan taulukon viimeinen, 
      // tehdään viimeinen jako keskiarvoa varten.
      // Viimeiseksi asetetaan uusi muuttuja uuden taulukon viimeisimmän objektin tilalle.
      if (index === array.length - 1) { newEntry[1] /= divider };
      powerOutputMonthAverages[powerOutputMonthAverages.length - 1] = newEntry;
    });

    let powerOutputDayValues = powerOutputMonthValues.slice(-144);

    let lastEntry = data.values[data.values.length - 1];

    let monthTotalProduction = powerOutputMonthValues.reduce((total, current) => {
      return current[1] != null ? total + current[1] : total;
    }, 0) / powerOutputMonthValues.length * 24 * 30 / 1000;

    let mappedData = {
      dayOutputChart: {
        options: {
          colors: [
            "#00a5d0"
          ],
          chart: {
            animations: {
              enabled: true,
              speed: 600,
              dynamicAnimation: {
                enabled: true,
                speed: 300,
              }
            },
            id: "dayOutputChart",
            locales: [fi],
            defaultLocale: "fi",
            toolbar: {
              show: false
            },
            zoom: {
              enabled: false
            }
          },
          stroke: {
            curve: "smooth",
            width: 2,
          },
          tooltip: {
            enabled: false
          },
          xaxis: {
            type: "datetime",
            labels: {
              style: {
                fontFamily: "Quicksand, sans-serif",
                fontSize: "14px"
              },
              datetimeFormatter: {
                hour: "HH.mm",
              }
            },
          },
          yaxis: {
            tickAmount: 4,
            decimalsInFloat: 0,
            forceNiceScale: true,
            labels: {
              style: {
                fontFamily: "Quicksand, sans-serif",
                fontSize: "14px"
              },
              formatter: value => { return value + " W" },
            },
          }
        },
        series: [{
          name: "energiateho",
          type: "area",
          data: powerOutputDayValues,
        }]
      },
      monthOutputChart: {
        options: {
          colors: [
            "#00a5d0"
          ],
          chart: {
            animations: {
              enabled: true,
              speed: 600,
              dynamicAnimation: {
                enabled: true,
                speed: 300,
              }
            },
            id: "dayOutputChart",
            locales: [fi],
            defaultLocale: "fi",
            toolbar: {
              show: false
            },
            zoom: {
              enabled: false
            }
          },
          stroke: {
            curve: "smooth",
            width: 2,
          },
          tooltip: {
            enabled: false
          },
          xaxis: {
            type: "datetime",
            labels: {
              style: {
                fontFamily: "Quicksand, sans-serif",
                fontSize: "14px"
              },
              formatter: (value, timestamp, index) => {
                return moment(timestamp).format("DD.M.");
              }
            },
            tickAmount: 14
          },
          yaxis: {
            min: 0,
            forceNiceScale: true,
            decimalsInFloat: 0,
            labels: {
              style: {
                fontFamily: "Quicksand, sans-serif",
                fontSize: "14px"
              },
              formatter: value => { return Math.round(value) + " W" },
            },
          }
        },
        series: [{
          name: "energiateho",
          type: "area",
          data: powerOutputMonthAverages,
        }]
      },
      lastEntry: lastEntry,
      monthTotalProduction: monthTotalProduction
    }
    return mappedData;
  }
}

export default EnervizView;