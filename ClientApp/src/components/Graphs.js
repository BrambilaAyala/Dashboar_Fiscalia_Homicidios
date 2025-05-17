import React, { Component } from 'react';
import { BarChart, Bar, LineChart, Line, XAxis, YAxis, Tooltip, Legend, ResponsiveContainer, CartesianGrid, AreaChart, Area } from 'recharts';

export class Graphs extends Component {
  static displayName = Graphs.name;

  constructor(props) {
    super(props);
    this.state = {
      datosHomicidios: [],
      datosHomicidiosPorTrimestre: [],
      loading: true,
      variableSeleccionada: 'num_Muertos',
      añoSeleccionado: '' // <- nuevo estado para filtro por año
    };
  }

  componentDidMount() {
    this.fetchData();
    this.fetchDataPorTrimestre();
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
      const datosOrdenados = data.sort((a, b) => new Date(a.Fecha) - new Date(b.Fecha));
      console.log('Datos recibidos:', datosOrdenados);
      this.setState({ datosHomicidios: datosOrdenados, loading: false });

    } catch (error) {
      console.error('Error en la petición:', error);
      this.setState({ datosHomicidios: [], loading: false });
    }
  }

  async fetchDataPorTrimestre() {
    try {
      const response = await fetch('https://localhost:44497/LectorPdf/homicidios-por-trimestre', {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
      });

      if (!response.ok) throw new Error('Network response was not ok');

      const data = await response.json();
      console.log('Datos por trimestre recibidos:', data);
      this.setState({ datosHomicidiosPorTrimestre: data });

    } catch (error) {
      console.error('Error en la petición:', error);
      this.setState({ datosHomicidiosPorTrimestre: [] });
    }
  }

  handleChange = (event) => {
    this.setState({ variableSeleccionada: event.target.value });
  };

  render() {
    const { datosHomicidios, datosHomicidiosPorTrimestre, loading, variableSeleccionada, añoSeleccionado } = this.state;

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

    // 🔎 Obtener los años únicos disponibles
    const añosDisponibles = [...new Set(datosHomicidiosPorTrimestre.map(d => d.trimestre.split('-')[0]))];

    // 📊 Filtrar por año si se seleccionó uno
    const datosFiltradosPorAño = añoSeleccionado
    ? datosHomicidiosPorTrimestre.filter(d => d.trimestre.startsWith(añoSeleccionado))
    : datosHomicidiosPorTrimestre;
  
  

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
                  <XAxis dataKey="fecha" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Area type="monotone" dataKey="diferencia" fill="#8884d8" stroke="#8884d8" name="Diferencia (Hombres - Mujeres)" />
                </AreaChart>
              ) : variableSeleccionada === 'comparacion' ? (
                <LineChart data={datosConDiferencia} margin={{ top: 20, right: 30, left: 20, bottom: 10 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="fecha" />
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
                  <XAxis dataKey="fecha" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line type="monotone" dataKey={variableSeleccionada} stroke="#8884d8" name={opciones.find(op => op.key === variableSeleccionada)?.label} />
                </LineChart>
              )}
            </ResponsiveContainer>
          </>
        )}

        <h1>Homicidios por Trimestre (Últimos tres años)</h1>
        {!loading && (
          <>
            <label>Selecciona el año: </label>
            <select value={añoSeleccionado} onChange={(e) => this.setState({ añoSeleccionado: e.target.value })}>
              <option value="">Todos</option>
              {añosDisponibles.map(año => (
                <option key={año} value={año}>{año}</option>
              ))}
            </select>

            <ResponsiveContainer width="100%" height={400}>
              <BarChart data={datosFiltradosPorAño} margin={{ top: 20, right: 30, left: 20, bottom: 10 }}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="trimestre" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="hombres" fill="#8884d8" name="Hombres" />
                <Bar dataKey="mujeres" fill="#29c22b" name="Mujeres" />
                <Bar dataKey="no_Identificado" fill="#c22930" name="No Identificado" />
              </BarChart>
            </ResponsiveContainer>
          </>
        )}
      </div>
    );
  }
}
