import React, { Component } from 'react';
import { Chart } from 'chart.js/auto';

export class PruebaDash extends Component {
  static displayName = PruebaDash.name;

  state = {
    data: []
  };

  componentDidMount() {
    this.fetchData();
  }

  fetchData = async () => {
    const response = await fetch('/LectorPdfOcr');
    const data = await response.json();
    this.setState({ data }, this.createChart);
  };

  
  createChart() {
    const ctx = this.chartRef.getContext('2d');
    new Chart(ctx, {
      type: 'bar',
      data: {
        labels: [
          'Morelos', 'Aguascalientes', 'Baja California', 'Nayarit', 'Baja California Sur', 'Nuevo León', 
          'Campeche', 'Oaxaca', 'Chiapas', 'Puebla', 'Chihuahua', 'Querétaro', 
          'Ciudad de México', 'Quintana Roo', 'Coahuila', 'San Luis Potosí', 'Colima', 'Sinaloa', 'Durango', 
          'Sonora', 'Estado de México', 'Tabasco', 
          'Guanajuato', 'Tamaulipas', 'Guerrero', 
          'Tlaxcala', 'Hidalgo', 'Veracruz', 'Jalisco', 'Yucatán', 'Michoacán', 
           'Zacatecas'
        ],
        datasets: [{
          label: '# de muertes',
          data:  this.state.data,
          backgroundColor: [
            'rgba(255, 99, 132, 0.2)',
            'rgba(54, 162, 235, 0.2)',
            'rgba(255, 206, 86, 0.2)',
            'rgba(75, 192, 192, 0.2)',
            'rgba(153, 102, 255, 0.2)',
            'rgba(255, 159, 64, 0.2)',
          ],
          borderColor: [
            'rgba(255, 99, 132, 1)',
            'rgba(54, 162, 235, 1)',
            'rgba(255, 206, 86, 1)',
            'rgba(75, 192, 192, 1)',
            'rgba(153, 102, 255, 1)',
            'rgba(255, 159, 64, 1)',
          ],
          borderWidth: 1,
        }],
      },
      options: {
        scales: {
          y: {
            beginAtZero: true,
          },
        },
      },
    });
  }

  render() {
    return (
      <div>
        <canvas ref={(ref) => this.chartRef = ref} id="myChart" width="400" height="190"></canvas>
      </div>
    );
  }
}
