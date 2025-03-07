import React, { Component } from 'react';
import { LineChart, Line, XAxis, YAxis, Tooltip, Legend, ResponsiveContainer, CartesianGrid, AreaChart, Area } from 'recharts';

export class Graphs extends Component {
  static displayName = Graphs.name;

  constructor(props) {
    super(props);
    this.state = { 
      datosHomicidios: [], 
      loading: true, 
      variableSeleccionada: 'num_Muertos' 
    };
  }

  componentDidMount() {
    this.fetchData();
  }

  async fetchData() {
    try {
      const response = await fetch('https://localhost:44497/LectorPdf', {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
      });

      if (!response.ok) throw new Error('Network response was not ok');

      const data = await response.json();
      console.log('Datos recibidos:', data);
      this.setState({ datosHomicidios: data, loading: false });

    } catch (error) {
      console.error('Error en la petición:', error);
      this.setState({ datosHomicidios: [], loading: false });
    }
  }

  handleChange = (event) => {
    this.setState({ variableSeleccionada: event.target.value });
  };

  render() {
    const { datosHomicidios, loading, variableSeleccionada } = this.state;
    
    const opciones = [
      { key: 'num_Muertos', label: 'Total Homicidios' },
      { key: 'hombres', label: 'Hombres' },
      { key: 'mujeres', label: 'Mujeres' },
      { key: 'no_Identificado', label: 'No Identificado' },
      { key: 'diferencia', label: 'Diferencia (Hombres - Mujeres)' },
      { key: 'comparacion', label: 'Comparación de los datos' }
    ];
    
    const datosConDiferencia = datosHomicidios.map(item => ({
      ...item,
      diferencia: (item.hombres ?? 0) - (item.mujeres ?? 0)
    }));

    return (
      <div>
        <h1>Gráfica de Homicidios</h1>
        {loading ? <p><em>Loading...</em></p> : (
          <>
            <label>Selecciona qué graficar: </label>
            <select value={variableSeleccionada} onChange={this.handleChange}>
              {opciones.map(opcion => (
                <option key={opcion.key} value={opcion.key}>{opcion.label}</option>
              ))}
            </select>
    
            <ResponsiveContainer width="100%" height={400}>
              {variableSeleccionada === 'diferencia' ? (
                <AreaChart data={datosConDiferencia} margin={{ top: 20, right: 30, left: 20, bottom: 10 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="Entidad" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Area type="monotone" dataKey="diferencia" fill="#8884d8" stroke="#8884d8" name="Diferencia (Hombres - Mujeres)" />
                </AreaChart>
              ) : variableSeleccionada === 'comparacion' ? (
                <LineChart data={datosConDiferencia} margin={{ top: 20, right: 30, left: 20, bottom: 10 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="Entidad" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line type="monotone" dataKey="hombres" stroke="#8884d8" name="Hombres" />
                  <Line type="monotone" dataKey="mujeres" stroke="#29c22b" name="Mujeres" />
                  <Line type="monotone" dataKey="no_Identificado" stroke="#c22930" name="No Identificado" />
                  
                </LineChart>
              ) : (
                <LineChart data={datosConDiferencia} margin={{ top: 20, right: 30, left: 20, bottom: 10 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="Entidad" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line type="monotone" dataKey={variableSeleccionada} stroke="#8884d8" name={opciones.find(op => op.key === variableSeleccionada)?.label} />
                </LineChart>
              )}
            </ResponsiveContainer>
          </>
        )}
      </div>
    );
  }
}