import React, { Component } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid,
  PieChart, Pie, Cell, Legend
} from 'recharts';
import './Fisca.css';

const COLORES_GENERO = ['#3b82f6', '#ec4899', '#f59e0b']; // Hombres, Mujeres, No Identificado

export class Fisca extends Component {
  static displayName = Fisca.name;

  constructor(props) {
    super(props);
    this.state = {
      datosHomicidios: [],
      datosFiltrados: [],
      loading: true,
      filtroTexto: '',
      filtroEntidad: 'TODAS',
      filtroAño: 'TODOS',
      paginaActual: 1,
      elementosPorPagina: 50, // 25, 50, 100, o 'TODOS'
      // Panel de Ingesta / ETL
      mostrarPanelEtl: false,
      etlFechaInicio: '2024-03-01',
      etlFechaFin: '2024-03-07',
      procesandoEtl: false,
      resultadoEtl: null
    };
  }

  componentDidMount() {
    this.fetchData();
  }

  fetchData = async () => {
    this.setState({ loading: true });
    try {
      const response = await fetch('/LectorPdf', {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
      });

      if (!response.ok) throw new Error('Error al consultar datos');

      const data = await response.json();
      this.setState({
        datosHomicidios: data,
        datosFiltrados: data,
        loading: false
      }, this.aplicarFiltros);
    } catch (error) {
      console.error('Error al obtener datos en Fisca:', error);
      this.setState({ datosHomicidios: [], datosFiltrados: [], loading: false });
    }
  };

  handleFiltroTexto = (e) => {
    const texto = e.target.value.toLowerCase();
    this.setState({ filtroTexto: texto, paginaActual: 1 }, this.aplicarFiltros);
  };

  handleFiltroEntidad = (e) => {
    const entidad = e.target.value;
    this.setState({ filtroEntidad: entidad, paginaActual: 1 }, this.aplicarFiltros);
  };

  handleFiltroAño = (e) => {
    const año = e.target.value;
    this.setState({ filtroAño: año, paginaActual: 1 }, this.aplicarFiltros);
  };

  handleElementosPorPagina = (e) => {
    const val = e.target.value === 'TODOS' ? 'TODOS' : parseInt(e.target.value, 10);
    this.setState({ elementosPorPagina: val, paginaActual: 1 });
  };

  aplicarFiltros = () => {
    const { datosHomicidios, filtroTexto, filtroEntidad, filtroAño } = this.state;

    let resultado = datosHomicidios.filter(item => {
      // Filtro de Entidad
      const cumpleEntidad = filtroEntidad === 'TODAS' ||
        (item.entidad && item.entidad.toUpperCase() === filtroEntidad.toUpperCase());

      // Filtro de Año (2023-2026)
      const fechaStr = item.fecha || '';
      const cumpleAño = filtroAño === 'TODOS' || fechaStr.startsWith(filtroAño);

      // Filtro de Texto (Municipio, Entidad, Fuente)
      const cumpleTexto = !filtroTexto ||
        (item.municipio && item.municipio.toLowerCase().includes(filtroTexto)) ||
        (item.entidad && item.entidad.toLowerCase().includes(filtroTexto)) ||
        (item.fuente && item.fuente.toLowerCase().includes(filtroTexto));

      return cumpleEntidad && cumpleAño && cumpleTexto;
    });

    this.setState({ datosFiltrados: resultado });
  };

  cambiarPagina = (nuevaPagina) => {
    this.setState({ paginaActual: nuevaPagina });
  };

  ejecutarEtlRango = async () => {
    const { etlFechaInicio, etlFechaFin } = this.state;
    if (!etlFechaInicio || !etlFechaFin) return;

    this.setState({ procesandoEtl: true, resultadoEtl: null });
    try {
      const response = await fetch('/api/etl/procesar-rango', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          fechaInicio: etlFechaInicio,
          fechaFin: etlFechaFin
        })
      });

      if (!response.ok) throw new Error('Error al ejecutar el pipeline ETL');

      const data = await response.json();
      this.setState({ procesandoEtl: false, resultadoEtl: data });
      // Recargar datos para reflejar los nuevos registros
      this.fetchData();
    } catch (err) {
      this.setState({
        procesandoEtl: false,
        resultadoEtl: { exitoso: false, mensaje: `Error: ${err.message}` }
      });
    }
  };

  exportarCSV = () => {
    const { datosFiltrados } = this.state;
    if (datosFiltrados.length === 0) return;

    const encabezados = ['Municipio', 'Entidad', 'Homicidios', 'Hombres', 'Mujeres', 'No Identificado', 'Fuente', 'Fecha'];
    const filas = datosFiltrados.map(d => [
      `"${d.municipio || ''}"`,
      `"${d.entidad || ''}"`,
      d.num_Muertos || 0,
      d.hombres || 0,
      d.mujeres || 0,
      d.no_Identificado || 0,
      `"${d.fuente || ''}"`,
      `"${d.fecha || ''}"`
    ]);

    const contenido = [encabezados.join(','), ...filas.map(f => f.join(','))].join('\n');
    const blob = new Blob([contenido], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `homicidios_fiscalia_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  render() {
    const {
      datosHomicidios, datosFiltrados, loading,
      filtroTexto, filtroEntidad, filtroAño, paginaActual, elementosPorPagina,
      mostrarPanelEtl, etlFechaInicio, etlFechaFin, procesandoEtl, resultadoEtl
    } = this.state;

    if (loading) {
      return (
        <div style={{ textAlign: 'center', padding: '60px' }}>
          <h3><em>Cargando centro de análisis criminal...</em></h3>
        </div>
      );
    }

    // Métricas globales sobre registros filtrados
    const totalHomicidios = datosFiltrados.reduce((acc, cur) => acc + (cur.num_Muertos || 0), 0);
    const totalHombres = datosFiltrados.reduce((acc, cur) => acc + (cur.hombres || 0), 0);
    const totalMujeres = datosFiltrados.reduce((acc, cur) => acc + (cur.mujeres || 0), 0);
    const totalNoId = datosFiltrados.reduce((acc, cur) => acc + (cur.no_Identificado || 0), 0);

    // Ajuste coherente de género: si el total de géneros es 0 pero hay homicidios, asignarlos a No Identificado
    let hombresAjustados = totalHombres;
    let mujeresAjustadas = totalMujeres;
    let noIdAjustado = totalNoId;

    const sumaGeneros = hombresAjustados + mujeresAjustadas + noIdAjustado;
    if (totalHomicidios > 0 && sumaGeneros === 0) {
      noIdAjustado = totalHomicidios;
    } else if (totalHomicidios > sumaGeneros) {
      noIdAjustado += (totalHomicidios - sumaGeneros);
    }

    const baseCalculo = (hombresAjustados + mujeresAjustadas + noIdAjustado) || (totalHomicidios > 0 ? totalHomicidios : 1);
    const porcHombres = baseCalculo > 0 ? ((hombresAjustados / baseCalculo) * 100).toFixed(1) : '0.0';
    const porcMujeres = baseCalculo > 0 ? ((mujeresAjustadas / baseCalculo) * 100).toFixed(1) : '0.0';
    const porcNoId = baseCalculo > 0 ? ((noIdAjustado / baseCalculo) * 100).toFixed(1) : '0.0';

    // Entidades únicas y Años únicos
    const entidadesUnicas = [...new Set(datosHomicidios.map(d => d.entidad).filter(Boolean))].sort();
    const añosUnicos = [...new Set(datosHomicidios.map(d => (d.fecha ? d.fecha.slice(0, 4) : '')).filter(Boolean))].sort((a, b) => b - a);

    // Agrupación real por Municipio / Entidad para obtener el Top 8 con incidencia acumulada
    const mapaMunicipios = new Map();

    datosFiltrados.forEach(d => {
      const munRaw = (d.municipio || 'ESTATAL').trim();
      const entRaw = (d.entidad || 'DESCONOCIDO').trim();

      // Etiqueta distintiva y legible para cada barra
      let etiqueta;
      if (munRaw.toUpperCase() === 'ESTATAL' || munRaw.toUpperCase() === 'NO ESPECIFICADO') {
        etiqueta = `${entRaw} (Estatal)`;
      } else {
        etiqueta = `${munRaw} (${entRaw.slice(0, 3)})`;
      }

      const clave = `${munRaw.toUpperCase()}-${entRaw.toUpperCase()}`;
      if (!mapaMunicipios.has(clave)) {
        mapaMunicipios.set(clave, {
          nombre: etiqueta,
          municipio: munRaw,
          entidad: entRaw,
          muertos: 0,
          hombres: 0,
          mujeres: 0,
          no_Identificado: 0
        });
      }

      const itemMun = mapaMunicipios.get(clave);
      itemMun.muertos += (d.num_Muertos || 0);
      itemMun.hombres += (d.hombres || 0);
      itemMun.mujeres += (d.mujeres || 0);
      itemMun.no_Identificado += (d.no_Identificado || 0);
    });

    const topMunicipios = Array.from(mapaMunicipios.values())
      .filter(m => m.muertos > 0)
      .sort((a, b) => b.muertos - a.muertos)
      .slice(0, 8);

    // Datos para gráfico de pastel de género
    const datosGeneroPie = [
      { name: 'Hombres', value: hombresAjustados },
      { name: 'Mujeres', value: mujeresAjustadas },
      { name: 'No Identificados', value: noIdAjustado }
    ].filter(d => d.value > 0);

    // Paginación o Modo Completo
    const esModoTodos = elementosPorPagina === 'TODOS';
    const totalPaginas = esModoTodos ? 1 : Math.ceil(datosFiltrados.length / elementosPorPagina) || 1;
    const indiceInicio = esModoTodos ? 0 : (paginaActual - 1) * elementosPorPagina;
    const elementosPaginados = esModoTodos
      ? datosFiltrados
      : datosFiltrados.slice(indiceInicio, indiceInicio + elementosPorPagina);

    return (
      <div style={{ padding: '20px', backgroundColor: '#f8fafc', minHeight: '100vh', fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif' }}>
        {/* ENCABEZADO */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '15px', marginBottom: '25px' }}>
          <div>
            <h1 style={{ margin: 0, fontSize: '28px', color: '#0f172a', fontWeight: 'bold' }}>
              Centro de Inteligencia y Registros Forenses (Fisca)
            </h1>
            <p style={{ margin: '5px 0 0 0', color: '#64748b', fontSize: '15px' }}>
              Consolidado de homicidios dolosos a nivel municipal, fuentes periciales e informes oficiales (2023 - 2026)
            </p>
          </div>

          <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
            <button
              onClick={() => this.setState({ mostrarPanelEtl: !mostrarPanelEtl })}
              style={{
                backgroundColor: '#4338ca',
                color: '#fff',
                border: 'none',
                padding: '10px 18px',
                borderRadius: '8px',
                cursor: 'pointer',
                fontWeight: '600',
                display: 'flex',
                alignItems: 'center',
                gap: '8px',
                boxShadow: '0 2px 6px rgba(67,56,202,0.3)'
              }}
            >
              📥 Ingesta Oficial CNS (2023-2026)
            </button>

            <button
              onClick={this.exportarCSV}
              style={{
                backgroundColor: '#0284c7',
                color: '#fff',
                border: 'none',
                padding: '10px 18px',
                borderRadius: '8px',
                cursor: 'pointer',
                fontWeight: '600',
                display: 'flex',
                alignItems: 'center',
                gap: '8px',
                boxShadow: '0 2px 6px rgba(2,132,199,0.3)'
              }}
            >
              📊 Exportar Datos (CSV)
            </button>
          </div>
        </div>

        {/* PANEL DESPLEGABLE DE EXTRACCIÓN Y PIPELINE ETL (2023 - 2026) */}
        {mostrarPanelEtl && (
          <div style={{ backgroundColor: '#fff', border: '2px solid #818cf8', borderRadius: '12px', padding: '20px', marginBottom: '25px', boxShadow: '0 4px 15px rgba(99,102,241,0.1)' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '15px' }}>
              <div>
                <h3 style={{ margin: 0, color: '#312e81', fontSize: '18px', fontWeight: '700' }}>
                  Extracción Automatizada de Boletines Oficiales (SSPC / CNS)
                </h3>
                <p style={{ margin: '4px 0 0 0', color: '#64748b', fontSize: '13px' }}>
                  Descarga directa de los reportes diarios PDF de homicidios desde <code>http://www.informeseguridad.cns.gob.mx/</code>.
                  <br />
                  <span style={{ color: '#059669', fontWeight: '600' }}>
                    ✓ Manejo multiformato 2023–2026 | ✓ Omite automáticamente archivos escaneados (imágenes puras) sin texto.
                  </span>
                </p>
              </div>
              <button
                onClick={() => this.setState({ mostrarPanelEtl: false })}
                style={{ background: 'none', border: 'none', fontSize: '18px', cursor: 'pointer', color: '#94a3b8' }}
              >
                ✕
              </button>
            </div>

            <div style={{ display: 'flex', gap: '15px', alignItems: 'center', flexWrap: 'wrap', marginBottom: '15px' }}>
              <div>
                <label style={{ fontSize: '12px', fontWeight: 'bold', color: '#475569', display: 'block', marginBottom: '4px' }}>
                  FECHA INICIAL:
                </label>
                <input
                  type="date"
                  value={etlFechaInicio}
                  onChange={(e) => this.setState({ etlFechaInicio: e.target.value })}
                  style={{ padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '14px' }}
                />
              </div>

              <div>
                <label style={{ fontSize: '12px', fontWeight: 'bold', color: '#475569', display: 'block', marginBottom: '4px' }}>
                  FECHA FINAL:
                </label>
                <input
                  type="date"
                  value={etlFechaFin}
                  onChange={(e) => this.setState({ etlFechaFin: e.target.value })}
                  style={{ padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '14px' }}
                />
              </div>

              {/* Accesos rápidos de rango */}
              <div style={{ alignSelf: 'flex-end', display: 'flex', gap: '6px' }}>
                {[
                  { label: '📅 Muestra 2026 (7 días)', i: '2026-08-24', f: '2026-08-30' },
                  { label: '📅 Muestra 2025 (7 días)', i: '2025-08-15', f: '2025-08-22' },
                  { label: '📅 Muestra 2024 (7 días)', i: '2024-03-01', f: '2024-03-07' },
                  { label: '📅 Muestra 2023 (7 días)', i: '2023-01-01', f: '2023-01-07' },
                  { label: '📅 Mes Reciente (30 días)', i: '2026-08-01', f: '2026-08-30' }
                ].map((preset, idx) => (
                  <button
                    key={idx}
                    onClick={() => this.setState({ etlFechaInicio: preset.i, etlFechaFin: preset.f })}
                    style={{
                      padding: '8px 10px',
                      fontSize: '12px',
                      borderRadius: '6px',
                      border: '1px solid #e2e8f0',
                      backgroundColor: '#f8fafc',
                      cursor: 'pointer',
                      fontWeight: '500'
                    }}
                  >
                    {preset.label}
                  </button>
                ))}
              </div>

              <div style={{ alignSelf: 'flex-end' }}>
                <button
                  onClick={this.ejecutarEtlRango}
                  disabled={procesandoEtl}
                  style={{
                    backgroundColor: procesandoEtl ? '#94a3b8' : '#059669',
                    color: '#fff',
                    border: 'none',
                    padding: '9px 20px',
                    borderRadius: '6px',
                    cursor: procesandoEtl ? 'not-allowed' : 'pointer',
                    fontWeight: 'bold',
                    fontSize: '14px',
                    boxShadow: '0 2px 5px rgba(5,150,105,0.3)'
                  }}
                >
                  {procesandoEtl ? '⏳ Descargando y Normalizando...' : '🚀 Iniciar Extracción Oficial'}
                </button>
              </div>
            </div>

            {/* Resultado de la Ingesta */}
            {resultadoEtl && (
              <div
                style={{
                  backgroundColor: resultadoEtl.exitoso ? '#ecfdf5' : '#fef2f2',
                  border: `1px solid ${resultadoEtl.exitoso ? '#a7f3d0' : '#fecaca'}`,
                  borderRadius: '8px',
                  padding: '12px 16px',
                  marginTop: '10px'
                }}
              >
                <strong style={{ color: resultadoEtl.exitoso ? '#065f46' : '#991b1b', fontSize: '14px' }}>
                  {resultadoEtl.exitoso ? '✅ Pipeline completado:' : '⚠️ Aviso en pipeline:'}
                </strong>
                <p style={{ margin: '4px 0 0 0', color: '#334155', fontSize: '13px' }}>
                  {resultadoEtl.mensaje}
                </p>
                {resultadoEtl.totalRegistrosGuardados > 0 && (
                  <span style={{ fontSize: '12px', color: '#059669', fontWeight: 'bold' }}>
                    + Se guardaron {resultadoEtl.totalRegistrosGuardados} nuevos registros en la base de datos MySQL.
                  </span>
                )}
              </div>
            )}
          </div>
        )}

        {/* TARJETAS DE MÉTRICAS (KPIs) */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '15px', marginBottom: '25px' }}>
          <div style={{ backgroundColor: '#fff', padding: '18px', borderRadius: '12px', boxShadow: '0 1px 4px rgba(0,0,0,0.05)', borderTop: '4px solid #ef4444' }}>
            <span style={{ fontSize: '13px', color: '#64748b', fontWeight: '600', textTransform: 'uppercase' }}>Total Víctimas</span>
            <div style={{ fontSize: '28px', fontWeight: 'bold', color: '#dc2626', marginTop: '4px' }}>
              {totalHomicidios.toLocaleString('es-MX')}
            </div>
            <small style={{ color: '#94a3b8' }}>En {datosFiltrados.length} registros seleccionados</small>
          </div>

          <div style={{ backgroundColor: '#fff', padding: '18px', borderRadius: '12px', boxShadow: '0 1px 4px rgba(0,0,0,0.05)', borderTop: '4px solid #3b82f6' }}>
            <span style={{ fontSize: '13px', color: '#64748b', fontWeight: '600', textTransform: 'uppercase' }}>Hombres</span>
            <div style={{ fontSize: '28px', fontWeight: 'bold', color: '#2563eb', marginTop: '4px' }}>
              {totalHombres.toLocaleString('es-MX')}
            </div>
            <small style={{ color: '#3b82f6', fontWeight: '600' }}>{porcHombres}% del total</small>
          </div>

          <div style={{ backgroundColor: '#fff', padding: '18px', borderRadius: '12px', boxShadow: '0 1px 4px rgba(0,0,0,0.05)', borderTop: '4px solid #ec4899' }}>
            <span style={{ fontSize: '13px', color: '#64748b', fontWeight: '600', textTransform: 'uppercase' }}>Mujeres</span>
            <div style={{ fontSize: '28px', fontWeight: 'bold', color: '#be185d', marginTop: '4px' }}>
              {totalMujeres.toLocaleString('es-MX')}
            </div>
            <small style={{ color: '#be185d', fontWeight: '600' }}>{porcMujeres}% del total</small>
          </div>

          <div style={{ backgroundColor: '#fff', padding: '18px', borderRadius: '12px', boxShadow: '0 1px 4px rgba(0,0,0,0.05)', borderTop: '4px solid #f59e0b' }}>
            <span style={{ fontSize: '13px', color: '#64748b', fontWeight: '600', textTransform: 'uppercase' }}>No Identificados</span>
            <div style={{ fontSize: '28px', fontWeight: 'bold', color: '#d97706', marginTop: '4px' }}>
              {totalNoId.toLocaleString('es-MX')}
            </div>
            <small style={{ color: '#94a3b8' }}>{porcNoId}% en proceso de peritaje</small>
          </div>

          <div style={{ backgroundColor: '#fff', padding: '18px', borderRadius: '12px', boxShadow: '0 1px 4px rgba(0,0,0,0.05)', borderTop: '4px solid #8b5cf6' }}>
            <span style={{ fontSize: '13px', color: '#64748b', fontWeight: '600', textTransform: 'uppercase' }}>Entidades Representadas</span>
            <div style={{ fontSize: '28px', fontWeight: 'bold', color: '#7c3aed', marginTop: '4px' }}>
              {entidadesUnicas.length} de 32
            </div>
            <small style={{ color: '#94a3b8' }}>Cobertura territorial activa</small>
          </div>
        </div>

        {/* GRÁFICAS DE RESUMEN: TOP MUNICIPIOS Y GÉNERO */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(350px, 1fr))', gap: '20px', marginBottom: '25px' }}>
          {/* Top Municipios */}
          <div style={{ backgroundColor: '#fff', padding: '20px', borderRadius: '12px', boxShadow: '0 2px 8px rgba(0,0,0,0.04)' }}>
            <h3 style={{ margin: '0 0 15px 0', fontSize: '16px', color: '#1e293b' }}>
              Municipios con Mayor Incidencia ({topMunicipios.length})
            </h3>
            <div style={{ width: '100%', height: 300 }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={topMunicipios} layout="vertical" margin={{ top: 5, right: 30, left: 15, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" horizontal={false} />
                  <XAxis type="number" />
                  <YAxis type="category" dataKey="nombre" width={150} tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Bar dataKey="muertos" name="Víctimas" fill="#ef4444" radius={[0, 4, 4, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Distribución de Género */}
          <div style={{ backgroundColor: '#fff', padding: '20px', borderRadius: '12px', boxShadow: '0 2px 8px rgba(0,0,0,0.04)' }}>
            <h3 style={{ margin: '0 0 15px 0', fontSize: '16px', color: '#1e293b' }}>
              Proporción por Género
            </h3>
            <div style={{ width: '100%', height: 260 }}>
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={datosGeneroPie}
                    cx="50%"
                    cy="50%"
                    innerRadius={50}
                    outerRadius={85}
                    paddingAngle={4}
                    dataKey="value"
                    label={({ name, percent }) => `${name} (${(percent * 100).toFixed(0)}%)`}
                  >
                    {datosGeneroPie.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORES_GENERO[index % COLORES_GENERO.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                  <Legend verticalAlign="bottom" height={36} />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>

        {/* BARRA DE HERRAMIENTAS Y FILTROS MULTIDIMENSIONALES */}
        <div style={{ backgroundColor: '#fff', padding: '16px 20px', borderRadius: '12px 12px 0 0', borderBottom: '1px solid #e2e8f0', display: 'flex', flexWrap: 'wrap', gap: '15px', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', alignItems: 'center' }}>
            {/* Buscador */}
            <input
              type="text"
              placeholder="🔍 Buscar municipio, estado, fuente..."
              value={filtroTexto}
              onChange={this.handleFiltroTexto}
              style={{
                padding: '8px 14px',
                borderRadius: '6px',
                border: '1px solid #cbd5e1',
                fontSize: '14px',
                minWidth: '260px'
              }}
            />

            {/* Selector de Entidad */}
            <select
              value={filtroEntidad}
              onChange={this.handleFiltroEntidad}
              style={{
                padding: '8px 14px',
                borderRadius: '6px',
                border: '1px solid #cbd5e1',
                fontSize: '14px',
                backgroundColor: '#fff',
                cursor: 'pointer'
              }}
            >
              <option value="TODAS">🏛️ Todas las Entidades ({entidadesUnicas.length})</option>
              {entidadesUnicas.map(ent => (
                <option key={ent} value={ent}>{ent}</option>
              ))}
            </select>

            {/* Filtro por Año (2023 - 2026) */}
            <select
              value={filtroAño}
              onChange={this.handleFiltroAño}
              style={{
                padding: '8px 14px',
                borderRadius: '6px',
                border: '1px solid #cbd5e1',
                fontSize: '14px',
                backgroundColor: '#fff',
                cursor: 'pointer',
                fontWeight: '600',
                color: '#334155'
              }}
            >
              <option value="TODOS">📅 Todos los Años (2023 - 2026)</option>
              {añosUnicos.map(año => (
                <option key={año} value={año}>Año {año}</option>
              ))}
            </select>
          </div>

          {/* Selector de Límite / Paginador */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <span style={{ fontSize: '13px', color: '#64748b' }}>
              Mostrando <strong>{elementosPaginados.length}</strong> de <strong>{datosFiltrados.length}</strong> registros
            </span>

            <label style={{ fontSize: '13px', color: '#475569', fontWeight: '600' }}>
              Filas por vista:
            </label>
            <select
              value={elementosPorPagina}
              onChange={this.handleElementosPorPagina}
              style={{
                padding: '6px 10px',
                borderRadius: '6px',
                border: '1px solid #cbd5e1',
                fontSize: '13px',
                backgroundColor: '#f8fafc',
                cursor: 'pointer',
                fontWeight: '600'
              }}
            >
              <option value={25}>25</option>
              <option value={50}>50</option>
              <option value={100}>100</option>
              <option value={250}>250</option>
              <option value="TODOS">Ver Todos (Sin límite)</option>
            </select>
          </div>
        </div>

        {/* TABLA PRINCIPAL DE DATOS FORENSES */}
        <div style={{ backgroundColor: '#fff', overflowX: 'auto', boxShadow: '0 4px 12px rgba(0,0,0,0.03)' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '14px' }}>
            <thead>
              <tr style={{ backgroundColor: '#f1f5f9', color: '#475569', borderBottom: '2px solid #e2e8f0' }}>
                <th style={{ padding: '12px 16px' }}>Municipio</th>
                <th style={{ padding: '12px 16px' }}>Entidad Federativa</th>
                <th style={{ padding: '12px 16px', textAlign: 'center' }}>Total Homicidios</th>
                <th style={{ padding: '12px 16px', textAlign: 'center' }}>Hombres</th>
                <th style={{ padding: '12px 16px', textAlign: 'center' }}>Mujeres</th>
                <th style={{ padding: '12px 16px', textAlign: 'center' }}>No Identificados</th>
                <th style={{ padding: '12px 16px' }}>Fuente Periodística / Oficial</th>
                <th style={{ padding: '12px 16px' }}>Fecha</th>
              </tr>
            </thead>
            <tbody>
              {elementosPaginados.length > 0 ? (
                elementosPaginados.map((item, idx) => (
                  <tr
                    key={item.id || idx}
                    style={{
                      borderBottom: '1px solid #f1f5f9',
                      transition: 'background-color 0.15s ease'
                    }}
                    onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = '#f8fafc')}
                    onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = 'transparent')}
                  >
                    <td style={{ padding: '12px 16px', fontWeight: '600', color: '#1e293b' }}>
                      {item.municipio}
                    </td>
                    <td style={{ padding: '12px 16px', color: '#475569' }}>
                      {item.entidad}
                    </td>
                    <td style={{ padding: '12px 16px', textAlign: 'center' }}>
                      <span style={{ backgroundColor: '#fee2e2', color: '#b91c1c', padding: '4px 10px', borderRadius: '12px', fontWeight: 'bold' }}>
                        {item.num_Muertos}
                      </span>
                    </td>
                    <td style={{ padding: '12px 16px', textAlign: 'center' }}>
                      <span style={{ backgroundColor: '#dbeafe', color: '#1d4ed8', padding: '3px 8px', borderRadius: '10px', fontSize: '13px', fontWeight: '500' }}>
                        {item.hombres}
                      </span>
                    </td>
                    <td style={{ padding: '12px 16px', textAlign: 'center' }}>
                      <span style={{ backgroundColor: '#fce7f3', color: '#be185d', padding: '3px 8px', borderRadius: '10px', fontSize: '13px', fontWeight: '500' }}>
                        {item.mujeres}
                      </span>
                    </td>
                    <td style={{ padding: '12px 16px', textAlign: 'center', color: '#64748b' }}>
                      {item.no_Identificado || 0}
                    </td>
                    <td style={{ padding: '12px 16px', color: '#64748b', fontSize: '13px' }}>
                      {item.fuente || 'Sin fuente'}
                    </td>
                    <td style={{ padding: '12px 16px', color: '#0f172a', fontSize: '13px', fontWeight: '500' }}>
                      {item.fecha || 'N/A'}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan="8" style={{ textAlign: 'center', padding: '40px', color: '#94a3b8' }}>
                    No se encontraron registros que coincidan con los filtros seleccionados.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {/* PAGINADOR (Solo si no está en modo 'TODOS') */}
        {!esModoTodos && (
          <div style={{ backgroundColor: '#fff', padding: '14px 20px', borderRadius: '0 0 12px 12px', borderTop: '1px solid #e2e8f0', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ fontSize: '13px', color: '#64748b' }}>
              Página <strong>{paginaActual}</strong> de <strong>{totalPaginas}</strong>
            </span>

            <div style={{ display: 'flex', gap: '6px' }}>
              <button
                onClick={() => this.cambiarPagina(paginaActual - 1)}
                disabled={paginaActual === 1}
                style={{
                  padding: '6px 14px',
                  borderRadius: '6px',
                  border: '1px solid #cbd5e1',
                  backgroundColor: paginaActual === 1 ? '#f1f5f9' : '#fff',
                  color: paginaActual === 1 ? '#94a3b8' : '#334155',
                  cursor: paginaActual === 1 ? 'not-allowed' : 'pointer'
                }}
              >
                ◀ Anterior
              </button>

              <button
                onClick={() => this.cambiarPagina(paginaActual + 1)}
                disabled={paginaActual === totalPaginas}
                style={{
                  padding: '6px 14px',
                  borderRadius: '6px',
                  border: '1px solid #cbd5e1',
                  backgroundColor: paginaActual === totalPaginas ? '#f1f5f9' : '#fff',
                  color: paginaActual === totalPaginas ? '#94a3b8' : '#334155',
                  cursor: paginaActual === totalPaginas ? 'not-allowed' : 'pointer'
                }}
              >
                Siguiente ▶
              </button>
            </div>
          </div>
        )}
      </div>
    );
  }
}
