import React, { Component } from 'react';
import './Fisca.css';

export class Fisca extends Component {
  static displayName = Fisca.name;

  constructor(props) {
    super(props);
    this.state = { datosHomicidios: [], loading: true };
  }

  componentDidMount() {
    this.fetchData();
  }

  async fetchData() {
    try {
      const response = await fetch('https://localhost:44497/LectorPdf', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // Solo si usas cookies o autenticación
      });
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }
      const data = await response.json();
      //console.log(data);
      console.log('Datos recibidos:', data);
      this.setState({ datosHomicidios: data, loading: false });
    } catch (error) {
      console.error('There was a problem with the fetch operation:', error);
      this.setState({ datosHomicidios: [], loading: false });
    }
  }

  render() {
    
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : (
          <div>
            <h2>Datos de Municipios, Entidades y Homicidios</h2>
            <table>
              <thead>
                <tr>
                  <th>Municipio</th>
                  <th>Entidad</th>
                  <th>Número de Muertos</th>
                  <th>Hombres</th>
                  <th>Mujeres</th>
                  <th>No Identificado</th>
                  <th>Fuente</th>
                </tr>
              </thead>
              <tbody>
                {this.state.datosHomicidios.slice(0, 30).map((item, index) => (
                  <tr key={index}>
                    <td>{item.municipio}</td>
                    <td>{item.entidad}</td>
                    <td>{item.num_Muertos}</td>
                    <td>{item.hombres}</td>
                    <td>{item.mujeres}</td>
                    <td>{item.no_Identificado}</td>
                    <td>{item.fuente}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        );
  
    return (
      <div>
        <h1 id="tableLabel">Contenido de los Homicidios</h1>
        {contents}
      </div>
    );
  }
}
  