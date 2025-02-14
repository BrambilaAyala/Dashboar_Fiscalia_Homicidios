import React, { Component } from 'react';
import { Chart, registerables } from 'chart.js';
import { ChoroplethController, GeoFeature, ProjectionScale, ColorScale } from 'chartjs-chart-geo';
import { feature } from 'topojson-client';

Chart.register(ChoroplethController, GeoFeature, ProjectionScale, ColorScale, ...registerables);

export class PruebaMapa extends Component {
  componentDidMount() {
    this.createChart();
  }

  state = {
    data: []
  };
  componentDidMount() {
    this.fetchData();
  }
  fetchData = async () => {
    const response = await fetch('/LectorPdfOcr');
    const data = await response.json();

    // Intercambiar los elementos en las posiciones 0 y 5 
    let temp = data[3]; 
    data[3] = data[10]; 
    data[10] = temp;

    this.setState({ data }, this.createChart);
  };
  
  async createChart() {
    const ctx = this.chartRef.getContext('2d');
    const response = await fetch('/mex.topo.json'); // Ruta al archivo JSON en la carpeta pública
    const mex = await response.json();

    const states = feature(mex, mex.objects.mex).features;

    //const datos = this.state.data;
    // Asignar los datos a cada estado 
    const dataWithValues = states.map((d, i) => ({ 
      feature: d, value: this.state.data[i] || 0 // Asignar valor de la lista o 0 si no hay valor 
      }));

    new Chart(ctx, {
      type: 'choropleth',
      data: {
        labels: states.map(d => d.properties.name),
        datasets: [{
          label: 'Estados de México',
          outline: states,
          data: dataWithValues
        }]
      },
      options: {
        plugins: {
          legend: {
            display: false
          }
        },
        scales: {
          projection: {
            axis: 'x',
            projection: 'mercator' // Ajusta la proyección según lo necesites
          },
          color: {
            axis: 'x',
            interpolate: 'ylOrRd',
            quantize: 5,
            legend: {
              position: 'bottom-right',
              align: 'bottom'
            }
          }
        }
      }
    });
  }

  render() {
    return (
      <div>
        <h1 style={{textAlign: 'center'}}>Homicidios dolosos en Mexico</h1>
        <canvas ref={(ref) => this.chartRef = ref} id="canvas" width="800" height="400"></canvas>
      </div>
    );
  }
}


// import React, { Component } from 'react';
// import { Chart, registerables } from 'chart.js';
// import { ChoroplethController, GeoFeature, ProjectionScale, ColorScale } from 'chartjs-chart-geo';
// import { feature } from 'topojson-client';

// Chart.register(ChoroplethController, GeoFeature, ProjectionScale, ColorScale, ...registerables);

// export class PruebaMapa extends Component {
//   componentDidMount() {
//     this.createChart();
//   }

//   async createChart() {
//     const ctx = this.chartRef.getContext('2d');
//     const response = await fetch('https://unpkg.com/us-atlas/states-10m.json');
//     const us = await response.json();

//     const nation = feature(us, us.objects.nation).features[0];
//     const states = feature(us, us.objects.states).features;

//     new Chart(ctx, {
//       type: 'choropleth',
//       data: {
//         labels: states.map((d) => d.properties.name),
//         datasets: [{
//           label: 'States',
//           outline: nation,
//           data: states.map((d) => ({ feature: d, value: Math.random() * 10 }))
//         }]
//       },
//       options: {
//         plugins: {
//           legend: {
//             display: false
//           }
//         },
//         scales: {
//           projection: {
//             axis: 'x',
//             projection: 'albersUsa'
//           },
//           color: {
//             axis: 'x',
//             quantize: 5,
//             legend: {
//               position: 'bottom-right',
//               align: 'bottom'
//             }
//           }
//         }
//       }
//     });
//   }

//   render() {
//     return (
//       <div>
//         <canvas ref={(ref) => this.chartRef = ref} id="canvas" width="800" height="400"></canvas>
//       </div>
//     );
//   }
// }
