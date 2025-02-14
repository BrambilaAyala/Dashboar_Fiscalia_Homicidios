import React, { Component } from 'react';



export class Home extends Component {
  static displayName = Home.name;

  render() {
    return (
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ flex: 1 }}>
          <h1>Homicidios dolosos</h1>
          {/*<p>Recopilacion de Homicidios Dolosos:</p>
          <ul>
            <li><a href='https://localhost:44497/fisca'>Tabla de Casos</a> </li>
            <li><a href='https://localhost:44497/pruebadash'>Grafico de Barras de Prueba</a></li>
            <li><a href='https://localhost:44497/graphs'>Graficos de Casos Registrados</a></li>
            <li><a href='https://localhost:44497/map'>Mapa de Casos Registrados</a></li>
          </ul>*/}
          <p><strong>Miebros:</strong></p>
          <ul>
            <li> Aldo Rafael Guzman Villanueva</li>
            <li>Francisco Javier Atondo Nubes</li>
            <li>Leonardo Brambila Ayala</li>
            <li>Manuel Alejandro Heredia Nogales</li>
          </ul>
          <p>Clase Analisis y Diseño Orientado a Objetos - 2024-2</p>
        </div>

        {/* Columna para la imagen */}
        <div style={{ flex: 1 }}>
        <img src="/Unison.png" style={{ width: '50%', height: 'auto' }} alt="Unison" />
        <img src="/gob.png" style={{ width: '50%', height: 'auto' }} alt="Unison" />
        </div>
      </div>
    );
  }
}