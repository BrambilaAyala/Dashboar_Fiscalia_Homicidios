import React, { Component } from 'react';
import {
  ResponsiveContainer,
  AreaChart,
  Area,
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  PieChart,
  Pie,
  Cell
} from 'recharts';

const COLORES = {
  total: '#6366f1',
  hombres: '#3b82f6',
  mujeres: '#ec4899',
  noIdentificado: '#f59e0b',
  diferencia: '#8b5cf6',
  grid: '#e2e8f0',
  fondoTooltip: '#0f172a'
};

const PALETA_PIE = ['#3b82f6', '#ec4899', '#f59e0b'];

export class Graphs extends Component {
  static displayName = Graphs.name;

  constructor(props) {
    super(props);
    this.state = {
      datosHomicidios: [],
      datosHomicidiosPorTrimestre: [],
      loading: true,
      error: null,
      variableSeleccionada: 'num_Muertos',
      tipoGrafico: 'area', // 'area', 'linea', 'barra'
      añoSeleccionado: '',
      añoTemporal: '',
      pestañaActiva: 'temporal' // 'temporal', 'trimestres', 'genero'
    };
  }

  componentDidMount() {
    this.cargarDatos();
  }

  cargarDatos = async () => {
    this.setState({ loading: true, error: null });
    try {
      const [resHomicidios, resTrimestres] = await Promise.all([
        fetch('/LectorPdf', {
          method: 'GET',
          headers: { 'Content-Type': 'application/json' },
          credentials: 'include',
        }),
        fetch('/LectorPdf/homicidios-por-trimestre', {
          method: 'GET',
          headers: { 'Content-Type': 'application/json' },
          credentials: 'include',
        })
      ]);

      let datosH = [];
      let datosT = [];

      if (resHomicidios.ok) {
        const jsonH = await resHomicidios.json();
        if (Array.isArray(jsonH)) {
          datosH = jsonH.sort((a, b) => new Date(a.fecha || a.Fecha) - new Date(b.fecha || b.Fecha));
        }
      }

      if (resTrimestres.ok) {
        const jsonT = await resTrimestres.json();
        if (Array.isArray(jsonT)) {
          datosT = jsonT;
        }
      }

      this.setState({
        datosHomicidios: datosH,
        datosHomicidiosPorTrimestre: datosT,
        loading: false
      });
    } catch (err) {
      console.error('Error cargando datos en Graphs:', err);
      this.setState({
        error: 'No se pudieron consultar los datos del servidor.',
        loading: false
      });
    }
  };

  renderTooltipPersonalizado = ({ active, payload, label }) => {
    if (active && payload && payload.length) {
      return (
        <div
          style={{
            backgroundColor: COLORES.fondoTooltip,
            color: '#fff',
            padding: '12px 16px',
            borderRadius: '8px',
            boxShadow: '0 8px 20px rgba(0,0,0,0.25)',
            fontSize: '13px',
            border: '1px solid #334155'
          }}
        >
          <div style={{ fontWeight: 'bold', marginBottom: '8px', borderBottom: '1px solid #334155', paddingBottom: '4px' }}>
            📅 {label}
          </div>
          {payload.map((item, index) => (
            <div key={index} style={{ display: 'flex', justifyContent: 'space-between', gap: '15px', margin: '4px 0' }}>
              <span style={{ color: item.color || '#fff' }}>● {item.name}:</span>
              <strong style={{ color: '#fff' }}>{Number(item.value || 0).toLocaleString('es-MX')}</strong>
            </div>
          ))}
        </div>
      );
    }
    return null;
  };

  render() {
    const {
      datosHomicidios,
      datosHomicidiosPorTrimestre,
      loading,
      error,
      variableSeleccionada,
      tipoGrafico,
      añoSeleccionado,
      añoTemporal,
      pestañaActiva
    } = this.state;

    // Normalizar nombres de llaves (minúsculas por compatibilidad ASP.NET JSON)
    const datosNormalizados = datosHomicidios.map(item => {
      const muertos = item.num_Muertos ?? item.Num_Muertos ?? 0;
      const hombres = item.hombres ?? item.Hombres ?? 0;
      const mujeres = item.mujeres ?? item.Mujeres ?? 0;
      const noId = item.no_Identificado ?? item.No_Identificado ?? 0;
      const fecha = item.fecha ?? item.Fecha ?? '';
      return {
        ...item,
        fecha: fecha ? fecha.slice(0, 10) : 'S/F',
        num_Muertos: muertos,
        hombres: hombres,
        mujeres: mujeres,
        no_Identificado: noId,
        diferencia: hombres - mujeres
      };
    });

    const datosTrimestresNormalizados = datosHomicidiosPorTrimestre.map(item => ({
      trimestre: item.trimestre ?? item.Trimestre ?? 'S/T',
      num_Muertos: item.num_Muertos ?? item.Num_Muertos ?? 0,
      hombres: item.hombres ?? item.Hombres ?? 0,
      mujeres: item.mujeres ?? item.Mujeres ?? 0,
      no_Identificado: item.no_Identificado ?? item.No_Identificado ?? 0,
      fecha: item.fecha ?? item.Fecha ?? ''
    }));

    // Métricas globales para los KPIs
    const totalHomicidios = datosNormalizados.reduce((acc, cur) => acc + (cur.num_Muertos || 0), 0);
    const totalHombres = datosNormalizados.reduce((acc, cur) => acc + (cur.hombres || 0), 0);
    const totalMujeres = datosNormalizados.reduce((acc, cur) => acc + (cur.mujeres || 0), 0);
    const totalNoId = datosNormalizados.reduce((acc, cur) => acc + (cur.no_Identificado || 0), 0);

    // Ajuste coherente de porcentajes de género (100% garantizado)
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
    const ratioGenero = mujeresAjustadas > 0 ? (hombresAjustados / mujeresAjustadas).toFixed(1) : 'N/A';

    // Identificar el trimestre pico
    let trimestrePico = { trimestre: 'Sin datos', num_Muertos: 0 };
    if (datosTrimestresNormalizados.length > 0) {
      trimestrePico = [...datosTrimestresNormalizados].sort((a, b) => b.num_Muertos - a.num_Muertos)[0] || trimestrePico;
    }

    // Años únicos disponibles en datos de homicidios
    const añosDisponiblesHomicidios = [
      ...new Set(
        datosNormalizados
          .map(d => (d.fecha ? d.fecha.slice(0, 4) : ''))
          .filter(a => a && !isNaN(a) && a.length === 4)
      )
    ].sort((a, b) => b - a);

    // Años únicos disponibles en datos por trimestre
    const añosDisponiblesTrimestres = [
      ...new Set(
        datosTrimestresNormalizados
          .map(d => (d.trimestre ? d.trimestre.split('-')[0].trim() : ''))
          .filter(a => a && !isNaN(a))
      )
    ].sort((a, b) => b - a);

    // Filtrar y consolidar la serie temporal por fecha cronológica (evita puntos duplicados en el eje X)
    const datosFiltradosPorAño = añoTemporal
      ? datosNormalizados.filter(d => d.fecha.startsWith(añoTemporal))
      : datosNormalizados;

    const mapaTemporal = new Map();
    datosFiltradosPorAño.forEach(d => {
      const fechaKey = d.fecha;
      if (!fechaKey || fechaKey === 'S/F') return;

      if (!mapaTemporal.has(fechaKey)) {
        mapaTemporal.set(fechaKey, {
          fecha: fechaKey,
          num_Muertos: 0,
          hombres: 0,
          mujeres: 0,
          no_Identificado: 0,
          diferencia: 0
        });
      }

      const p = mapaTemporal.get(fechaKey);
      p.num_Muertos += (d.num_Muertos || 0);
      p.hombres += (d.hombres || 0);
      p.mujeres += (d.mujeres || 0);
      p.no_Identificado += (d.no_Identificado || 0);
      p.diferencia = p.hombres - p.mujeres;
    });

    const serieTemporalConsolidada = Array.from(mapaTemporal.values())
      .sort((a, b) => new Date(a.fecha) - new Date(b.fecha));

    // Filtrar trimestres según el año seleccionado y ordenar cronológicamente
    const datosTrimestresFiltrados = añoSeleccionado
      ? datosTrimestresNormalizados.filter(d => d.trimestre.startsWith(añoSeleccionado))
      : datosTrimestresNormalizados;

    const datosTrimestresOrdenados = [...datosTrimestresFiltrados].sort((a, b) => {
      if (a.fecha && b.fecha) return new Date(a.fecha) - new Date(b.fecha);
      return a.trimestre.localeCompare(b.trimestre);
    });

    // Datos para gráfico de pastel de género
    const datosPieGenero = [
      { name: 'Hombres', value: hombresAjustados, color: COLORES.hombres },
      { name: 'Mujeres', value: mujeresAjustadas, color: COLORES.mujeres },
      { name: 'No Identificado', value: noIdAjustado, color: COLORES.noIdentificado }
    ].filter(d => d.value > 0);

    const opcionesVariables = [
      { key: 'num_Muertos', label: 'Total de Homicidios' },
      { key: 'comparacion', label: 'Comparativa por Género (H vs M vs No ID)' },
      { key: 'hombres', label: 'Víctimas Hombres' },
      { key: 'mujeres', label: 'Víctimas Mujeres' },
      { key: 'no_Identificado', label: 'No Identificado' },
      { key: 'diferencia', label: 'Brecha de Género (Hombres - Mujeres)' }
    ];

    return (
      <div style={{ padding: '24px', backgroundColor: '#f8fafc', minHeight: '100vh', fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif' }}>
        {/* ENCABEZADO */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '15px', marginBottom: '25px' }}>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <span
                style={{
                  backgroundColor: '#e0e7ff',
                  color: '#4338ca',
                  padding: '4px 10px',
                  borderRadius: '12px',
                  fontSize: '12px',
                  fontWeight: '700',
                  textTransform: 'uppercase',
                  letterSpacing: '0.5px'
                }}
              >
                Módulo Estadístico
              </span>
              <span style={{ color: '#94a3b8', fontSize: '13px' }}>SSPC • CNI • INEGI</span>
            </div>
            <h1 style={{ margin: '8px 0 4px 0', fontSize: '28px', color: '#0f172a', fontWeight: '800' }}>
              Tablero Analítico y Series de Tiempo
            </h1>
            <p style={{ margin: 0, color: '#64748b', fontSize: '15px' }}>
              Evolución cronológica, desglose estacional por trimestres y análisis demográfico de género.
            </p>
          </div>

          <button
            onClick={this.cargarDatos}
            style={{
              backgroundColor: '#fff',
              border: '1px solid #cbd5e1',
              padding: '9px 16px',
              borderRadius: '8px',
              cursor: 'pointer',
              fontWeight: '600',
              color: '#334155',
              display: 'flex',
              alignItems: 'center',
              gap: '6px',
              boxShadow: '0 1px 3px rgba(0,0,0,0.05)'
            }}
          >
            🔄 Actualizar Datos
          </button>
        </div>

        {/* TARJETAS DE MÉTRICAS EJECUTIVAS (KPIs) */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(210px, 1fr))', gap: '16px', marginBottom: '28px' }}>
          {/* TOTAL */}
          <div style={{ backgroundColor: '#fff', padding: '18px 20px', borderRadius: '12px', borderLeft: '5px solid #6366f1', boxShadow: '0 2px 5px rgba(0,0,0,0.04)' }}>
            <span style={{ fontSize: '12px', color: '#64748b', fontWeight: '700', textTransform: 'uppercase' }}>Total Víctimas</span>
            <div style={{ fontSize: '28px', fontWeight: '800', color: '#1e1b4b', marginTop: '4px' }}>
              {totalHomicidios.toLocaleString('es-MX')}
            </div>
            <div style={{ fontSize: '12px', color: '#94a3b8', marginTop: '2px' }}>Incidentes acumulados</div>
          </div>

          {/* HOMBRES */}
          <div style={{ backgroundColor: '#fff', padding: '18px 20px', borderRadius: '12px', borderLeft: '5px solid #3b82f6', boxShadow: '0 2px 5px rgba(0,0,0,0.04)' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontSize: '12px', color: '#64748b', fontWeight: '700', textTransform: 'uppercase' }}>Hombres</span>
              <span style={{ backgroundColor: '#dbeafe', color: '#1d4ed8', fontSize: '12px', fontWeight: '700', padding: '2px 8px', borderRadius: '10px' }}>
                {porcHombres}%
              </span>
            </div>
            <div style={{ fontSize: '28px', fontWeight: '800', color: '#1d4ed8', marginTop: '4px' }}>
              {totalHombres.toLocaleString('es-MX')}
            </div>
            <div style={{ fontSize: '12px', color: '#94a3b8', marginTop: '2px' }}>Víctimas masculinas</div>
          </div>

          {/* MUJERES */}
          <div style={{ backgroundColor: '#fff', padding: '18px 20px', borderRadius: '12px', borderLeft: '5px solid #ec4899', boxShadow: '0 2px 5px rgba(0,0,0,0.04)' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontSize: '12px', color: '#64748b', fontWeight: '700', textTransform: 'uppercase' }}>Mujeres</span>
              <span style={{ backgroundColor: '#fce7f3', color: '#be185d', fontSize: '12px', fontWeight: '700', padding: '2px 8px', borderRadius: '10px' }}>
                {porcMujeres}%
              </span>
            </div>
            <div style={{ fontSize: '28px', fontWeight: '800', color: '#be185d', marginTop: '4px' }}>
              {totalMujeres.toLocaleString('es-MX')}
            </div>
            <div style={{ fontSize: '12px', color: '#94a3b8', marginTop: '2px' }}>Víctimas femeninas</div>
          </div>

          {/* NO IDENTIFICADOS */}
          <div style={{ backgroundColor: '#fff', padding: '18px 20px', borderRadius: '12px', borderLeft: '5px solid #f59e0b', boxShadow: '0 2px 5px rgba(0,0,0,0.04)' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontSize: '12px', color: '#64748b', fontWeight: '700', textTransform: 'uppercase' }}>No Identificados</span>
              <span style={{ backgroundColor: '#fef3c7', color: '#b45309', fontSize: '12px', fontWeight: '700', padding: '2px 8px', borderRadius: '10px' }}>
                {porcNoId}%
              </span>
            </div>
            <div style={{ fontSize: '28px', fontWeight: '800', color: '#b45309', marginTop: '4px' }}>
              {totalNoId.toLocaleString('es-MX')}
            </div>
            <div style={{ fontSize: '12px', color: '#94a3b8', marginTop: '2px' }}>Pendientes de peritaje</div>
          </div>

          {/* TRIMESTRE PICO */}
          <div style={{ backgroundColor: '#fff', padding: '18px 20px', borderRadius: '12px', borderLeft: '5px solid #10b981', boxShadow: '0 2px 5px rgba(0,0,0,0.04)' }}>
            <span style={{ fontSize: '12px', color: '#64748b', fontWeight: '700', textTransform: 'uppercase' }}>Trimestre Pico</span>
            <div style={{ fontSize: '19px', fontWeight: '800', color: '#065f46', marginTop: '4px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
              {trimestrePico.trimestre}
            </div>
            <div style={{ fontSize: '12px', color: '#059669', fontWeight: '600', marginTop: '2px' }}>
              {trimestrePico.num_Muertos.toLocaleString('es-MX')} casos registrados
            </div>
          </div>
        </div>

        {/* NAVEGACIÓN POR PESTAÑAS / VISTAS */}
        <div style={{ display: 'flex', gap: '8px', borderBottom: '2px solid #e2e8f0', marginBottom: '25px' }}>
          {[
            { id: 'temporal', label: '📈 Serie Temporal e Histórica' },
            { id: 'trimestres', label: '🗓️ Análisis Trimestral & Estacional' },
            { id: 'genero', label: '👥 Demografía & Distribución de Género' }
          ].map(tab => (
            <button
              key={tab.id}
              onClick={() => this.setState({ pestañaActiva: tab.id })}
              style={{
                backgroundColor: 'transparent',
                border: 'none',
                borderBottom: pestañaActiva === tab.id ? '3px solid #6366f1' : '3px solid transparent',
                padding: '12px 18px',
                fontSize: '15px',
                fontWeight: pestañaActiva === tab.id ? '700' : '500',
                color: pestañaActiva === tab.id ? '#4338ca' : '#64748b',
                cursor: 'pointer',
                marginBottom: '-2px',
                transition: 'all 0.2s ease'
              }}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* MENSAJE DE CARGA O ERROR */}
        {loading && (
          <div style={{ backgroundColor: '#fff', borderRadius: '12px', padding: '60px', textAlign: 'center', boxShadow: '0 2px 5px rgba(0,0,0,0.04)' }}>
            <div style={{ fontSize: '32px', marginBottom: '10px' }}>⏳</div>
            <h3 style={{ color: '#334155', margin: 0 }}>Cargando datos analíticos del sistema...</h3>
          </div>
        )}

        {error && !loading && (
          <div style={{ backgroundColor: '#fef2f2', border: '1px solid #fecaca', borderRadius: '12px', padding: '20px', color: '#991b1b', marginBottom: '25px' }}>
            ⚠️ {error}
          </div>
        )}

        {/* PESTAÑA 1: SERIE TEMPORAL */}
        {!loading && pestañaActiva === 'temporal' && (
          <div style={{ backgroundColor: '#fff', borderRadius: '12px', padding: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.04)', marginBottom: '25px' }}>
            {/* CONTROLES DEL GRÁFICO */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '15px', marginBottom: '20px', paddingBottom: '16px', borderBottom: '1px solid #f1f5f9' }}>
              <div style={{ display: 'flex', gap: '15px', flexWrap: 'wrap', alignItems: 'center' }}>
                <div>
                  <label style={{ fontSize: '13px', fontWeight: '700', color: '#475569', marginRight: '8px', textTransform: 'uppercase' }}>
                    Variable:
                  </label>
                  <select
                    value={variableSeleccionada}
                    onChange={(e) => this.setState({ variableSeleccionada: e.target.value })}
                    style={{
                      padding: '8px 12px',
                      borderRadius: '8px',
                      border: '1px solid #cbd5e1',
                      fontSize: '13px',
                      backgroundColor: '#f8fafc',
                      fontWeight: '600',
                      color: '#1e293b',
                      cursor: 'pointer'
                    }}
                  >
                    {opcionesVariables.map(op => (
                      <option key={op.key} value={op.key}>{op.label}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label style={{ fontSize: '13px', fontWeight: '700', color: '#475569', marginRight: '8px', textTransform: 'uppercase' }}>
                    Filtrar Año:
                  </label>
                  <select
                    value={añoTemporal}
                    onChange={(e) => this.setState({ añoTemporal: e.target.value })}
                    style={{
                      padding: '8px 12px',
                      borderRadius: '8px',
                      border: '1px solid #cbd5e1',
                      fontSize: '13px',
                      backgroundColor: '#f8fafc',
                      fontWeight: '600',
                      color: '#1e293b',
                      cursor: 'pointer'
                    }}
                  >
                    <option value="">Todos los años (Histórico)</option>
                    {añosDisponiblesHomicidios.map(a => (
                      <option key={a} value={a}>Año {a}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div>
                <span style={{ fontSize: '13px', fontWeight: '700', color: '#475569', marginRight: '10px', textTransform: 'uppercase' }}>
                  Formato:
                </span>
                <div style={{ display: 'inline-flex', borderRadius: '8px', border: '1px solid #cbd5e1', overflow: 'hidden' }}>
                  {[
                    { key: 'area', label: 'Área' },
                    { key: 'linea', label: 'Línea' },
                    { key: 'barra', label: 'Barras' }
                  ].map(tipo => (
                    <button
                      key={tipo.key}
                      onClick={() => this.setState({ tipoGrafico: tipo.key })}
                      style={{
                        backgroundColor: tipoGrafico === tipo.key ? '#6366f1' : '#fff',
                        color: tipoGrafico === tipo.key ? '#fff' : '#475569',
                        border: 'none',
                        padding: '6px 14px',
                        fontSize: '13px',
                        fontWeight: '600',
                        cursor: 'pointer',
                        transition: 'background-color 0.15s ease'
                      }}
                    >
                      {tipo.label}
                    </button>
                  ))}
                </div>
              </div>
            </div>

            {/* GRÁFICO DINÁMICO */}
            <div style={{ width: '100%', height: 420 }}>
              <ResponsiveContainer width="100%" height="100%">
                {variableSeleccionada === 'comparacion' ? (
                  <LineChart data={serieTemporalConsolidada} margin={{ top: 20, right: 30, left: 10, bottom: 20 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke={COLORES.grid} vertical={false} />
                    <XAxis dataKey="fecha" stroke="#94a3b8" tick={{ fontSize: 12 }} />
                    <YAxis stroke="#94a3b8" tick={{ fontSize: 12 }} />
                    <Tooltip content={this.renderTooltipPersonalizado} />
                    <Legend wrapperStyle={{ paddingTop: '10px' }} />
                    <Line type="monotone" dataKey="hombres" name="Hombres" stroke={COLORES.hombres} strokeWidth={2.5} dot={{ r: 3 }} />
                    <Line type="monotone" dataKey="mujeres" name="Mujeres" stroke={COLORES.mujeres} strokeWidth={2.5} dot={{ r: 3 }} />
                    <Line type="monotone" dataKey="no_Identificado" name="No Identificado" stroke={COLORES.noIdentificado} strokeWidth={2} strokeDasharray="4 4" dot={{ r: 2 }} />
                  </LineChart>
                ) : variableSeleccionada === 'diferencia' ? (
                  <AreaChart data={serieTemporalConsolidada} margin={{ top: 20, right: 30, left: 10, bottom: 20 }}>
                    <defs>
                      <linearGradient id="colorDiferencia" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor={COLORES.diferencia} stopOpacity={0.5} />
                        <stop offset="95%" stopColor={COLORES.diferencia} stopOpacity={0.05} />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke={COLORES.grid} vertical={false} />
                    <XAxis dataKey="fecha" stroke="#94a3b8" tick={{ fontSize: 12 }} />
                    <YAxis stroke="#94a3b8" tick={{ fontSize: 12 }} />
                    <Tooltip content={this.renderTooltipPersonalizado} />
                    <Legend wrapperStyle={{ paddingTop: '10px' }} />
                    <Area type="monotone" dataKey="diferencia" name="Brecha de Género (Hombres - Mujeres)" stroke={COLORES.diferencia} strokeWidth={2.5} fillOpacity={1} fill="url(#colorDiferencia)" />
                  </AreaChart>
                ) : tipoGrafico === 'barra' ? (
                  <BarChart data={serieTemporalConsolidada} margin={{ top: 20, right: 30, left: 10, bottom: 20 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke={COLORES.grid} vertical={false} />
                    <XAxis dataKey="fecha" stroke="#94a3b8" tick={{ fontSize: 12 }} />
                    <YAxis stroke="#94a3b8" tick={{ fontSize: 12 }} />
                    <Tooltip content={this.renderTooltipPersonalizado} />
                    <Legend wrapperStyle={{ paddingTop: '10px' }} />
                    <Bar
                      dataKey={variableSeleccionada}
                      name={opcionesVariables.find(o => o.key === variableSeleccionada)?.label || variableSeleccionada}
                      fill={COLORES[variableSeleccionada] || COLORES.total}
                      radius={[4, 4, 0, 0]}
                    />
                  </BarChart>
                ) : tipoGrafico === 'linea' ? (
                  <LineChart data={serieTemporalConsolidada} margin={{ top: 20, right: 30, left: 10, bottom: 20 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke={COLORES.grid} vertical={false} />
                    <XAxis dataKey="fecha" stroke="#94a3b8" tick={{ fontSize: 12 }} />
                    <YAxis stroke="#94a3b8" tick={{ fontSize: 12 }} />
                    <Tooltip content={this.renderTooltipPersonalizado} />
                    <Legend wrapperStyle={{ paddingTop: '10px' }} />
                    <Line
                      type="monotone"
                      dataKey={variableSeleccionada}
                      name={opcionesVariables.find(o => o.key === variableSeleccionada)?.label || variableSeleccionada}
                      stroke={COLORES[variableSeleccionada] || COLORES.total}
                      strokeWidth={3}
                      dot={{ r: 3 }}
                    />
                  </LineChart>
                ) : (
                  <AreaChart data={serieTemporalConsolidada} margin={{ top: 20, right: 30, left: 10, bottom: 20 }}>
                    <defs>
                      <linearGradient id="colorVar" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor={COLORES[variableSeleccionada] || COLORES.total} stopOpacity={0.6} />
                        <stop offset="95%" stopColor={COLORES[variableSeleccionada] || COLORES.total} stopOpacity={0.05} />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke={COLORES.grid} vertical={false} />
                    <XAxis dataKey="fecha" stroke="#94a3b8" tick={{ fontSize: 12 }} />
                    <YAxis stroke="#94a3b8" tick={{ fontSize: 12 }} />
                    <Tooltip content={this.renderTooltipPersonalizado} />
                    <Legend wrapperStyle={{ paddingTop: '10px' }} />
                    <Area
                      type="monotone"
                      dataKey={variableSeleccionada}
                      name={opcionesVariables.find(o => o.key === variableSeleccionada)?.label || variableSeleccionada}
                      stroke={COLORES[variableSeleccionada] || COLORES.total}
                      strokeWidth={2.5}
                      fillOpacity={1}
                      fill="url(#colorVar)"
                    />
                  </AreaChart>
                )}
              </ResponsiveContainer>
            </div>
          </div>
        )}

        {/* PESTAÑA 2: ANÁLISIS TRIMESTRAL */}
        {!loading && pestañaActiva === 'trimestres' && (
          <div style={{ backgroundColor: '#fff', borderRadius: '12px', padding: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.04)', marginBottom: '25px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '15px', marginBottom: '20px', paddingBottom: '16px', borderBottom: '1px solid #f1f5f9' }}>
              <div>
                <h3 style={{ margin: 0, fontSize: '18px', color: '#0f172a' }}>Comportamiento Estacional por Trimestre</h3>
                <p style={{ margin: '4px 0 0 0', color: '#64748b', fontSize: '13px' }}>
                  Agrupación trimestral para la detección de patrones cíclicos e impacto en periodos vacacionales o invernales.
                </p>
              </div>

              <div>
                <label style={{ fontSize: '13px', fontWeight: '700', color: '#475569', marginRight: '10px' }}>
                  FILTRAR POR AÑO:
                </label>
                <select
                  value={añoSeleccionado}
                  onChange={(e) => this.setState({ añoSeleccionado: e.target.value })}
                  style={{
                    padding: '8px 14px',
                    borderRadius: '8px',
                    border: '1px solid #cbd5e1',
                    fontSize: '14px',
                    backgroundColor: '#f8fafc',
                    fontWeight: '600',
                    color: '#1e293b',
                    cursor: 'pointer'
                  }}
                >
                  <option value="">Todos los años</option>
                  {añosDisponiblesTrimestres.map(año => (
                    <option key={año} value={año}>Año {año}</option>
                  ))}
                </select>
              </div>
            </div>

            {/* GRÁFICO TRIMESTRAL DE BARRAS APILADAS O AGRUPADAS */}
            <div style={{ width: '100%', height: 380, marginBottom: '30px' }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={datosTrimestresOrdenados} margin={{ top: 20, right: 30, left: 10, bottom: 20 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke={COLORES.grid} vertical={false} />
                  <XAxis dataKey="trimestre" stroke="#94a3b8" tick={{ fontSize: 12 }} />
                  <YAxis stroke="#94a3b8" tick={{ fontSize: 12 }} />
                  <Tooltip content={this.renderTooltipPersonalizado} />
                  <Legend wrapperStyle={{ paddingTop: '10px' }} />
                  <Bar dataKey="hombres" fill={COLORES.hombres} name="Hombres" radius={[3, 3, 0, 0]} />
                  <Bar dataKey="mujeres" fill={COLORES.mujeres} name="Mujeres" radius={[3, 3, 0, 0]} />
                  <Bar dataKey="no_Identificado" fill={COLORES.noIdentificado} name="No Identificado" radius={[3, 3, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>

            {/* TABLA RESUMEN TRIMESTRAL */}
            <div style={{ marginTop: '20px' }}>
              <h4 style={{ margin: '0 0 12px 0', fontSize: '15px', color: '#1e293b' }}>Resumen Numérico de Trimestres</h4>
              <div style={{ overflowX: 'auto', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '14px' }}>
                  <thead>
                    <tr style={{ backgroundColor: '#f1f5f9', color: '#475569', textAlign: 'left' }}>
                      <th style={{ padding: '10px 14px', borderBottom: '1px solid #cbd5e1' }}>Trimestre / Periodo</th>
                      <th style={{ padding: '10px 14px', borderBottom: '1px solid #cbd5e1', textAlign: 'right' }}>Total Víctimas</th>
                      <th style={{ padding: '10px 14px', borderBottom: '1px solid #cbd5e1', textAlign: 'right' }}>Hombres</th>
                      <th style={{ padding: '10px 14px', borderBottom: '1px solid #cbd5e1', textAlign: 'right' }}>Mujeres</th>
                      <th style={{ padding: '10px 14px', borderBottom: '1px solid #cbd5e1', textAlign: 'right' }}>No Identificado</th>
                      <th style={{ padding: '10px 14px', borderBottom: '1px solid #cbd5e1' }}>Distribución</th>
                    </tr>
                  </thead>
                  <tbody>
                    {datosTrimestresOrdenados.map((row, idx) => {
                      const totalFila = (row.num_Muertos || 0);
                      const pctH = totalFila > 0 ? ((row.hombres / totalFila) * 100).toFixed(0) : 0;
                      const pctM = totalFila > 0 ? ((row.mujeres / totalFila) * 100).toFixed(0) : 0;

                      return (
                        <tr key={idx} style={{ borderBottom: '1px solid #f1f5f9', backgroundColor: idx % 2 === 0 ? '#fff' : '#f8fafc' }}>
                          <td style={{ padding: '10px 14px', fontWeight: '600', color: '#0f172a' }}>{row.trimestre}</td>
                          <td style={{ padding: '10px 14px', textAlign: 'right', fontWeight: '700', color: '#1e293b' }}>
                            {totalFila.toLocaleString('es-MX')}
                          </td>
                          <td style={{ padding: '10px 14px', textAlign: 'right', color: '#1d4ed8' }}>
                            {row.hombres?.toLocaleString('es-MX') || 0}
                          </td>
                          <td style={{ padding: '10px 14px', textAlign: 'right', color: '#be185d' }}>
                            {row.mujeres?.toLocaleString('es-MX') || 0}
                          </td>
                          <td style={{ padding: '10px 14px', textAlign: 'right', color: '#b45309' }}>
                            {row.no_Identificado?.toLocaleString('es-MX') || 0}
                          </td>
                          <td style={{ padding: '10px 14px', minWidth: '150px' }}>
                            <div style={{ display: 'flex', height: '8px', borderRadius: '4px', overflow: 'hidden', backgroundColor: '#e2e8f0' }}>
                              <div style={{ width: `${pctH}%`, backgroundColor: COLORES.hombres }} title={`Hombres: ${pctH}%`} />
                              <div style={{ width: `${pctM}%`, backgroundColor: COLORES.mujeres }} title={`Mujeres: ${pctM}%`} />
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}

        {/* PESTAÑA 3: DEMOGRAFÍA & GÉNERO */}
        {!loading && pestañaActiva === 'genero' && (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '20px', marginBottom: '25px' }}>
            {/* GRÁFICO DE DONA */}
            <div style={{ backgroundColor: '#fff', borderRadius: '12px', padding: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.04)' }}>
              <h3 style={{ margin: '0 0 6px 0', fontSize: '18px', color: '#0f172a' }}>Composición Porcentual por Género</h3>
              <p style={{ margin: '0 0 16px 0', color: '#64748b', fontSize: '13px' }}>
                Proporción directa sobre el universo de {totalHomicidios.toLocaleString('es-MX')} homicidios dolosos.
              </p>

              <div style={{ width: '100%', height: 300 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={datosPieGenero}
                      cx="50%"
                      cy="50%"
                      innerRadius={65}
                      outerRadius={105}
                      paddingAngle={4}
                      dataKey="value"
                      label={({ name, percent }) => `${name} (${(percent * 100).toFixed(1)}%)`}
                    >
                      {datosPieGenero.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={PALETA_PIE[index % PALETA_PIE.length]} />
                      ))}
                    </Pie>
                    <Tooltip content={this.renderTooltipPersonalizado} />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* TARJETAS DE HALLAZGOS DEMOGRÁFICOS */}
            <div style={{ backgroundColor: '#fff', borderRadius: '12px', padding: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.04)', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
              <div>
                <h3 style={{ margin: '0 0 6px 0', fontSize: '18px', color: '#0f172a' }}>Hallazgos Estadísticos Forenses</h3>
                <p style={{ margin: '0 0 20px 0', color: '#64748b', fontSize: '13px' }}>
                  Lectura criminológica de los datos consolidados:
                </p>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
                  <div style={{ backgroundColor: '#eff6ff', borderLeft: '4px solid #3b82f6', padding: '12px 14px', borderRadius: '6px' }}>
                    <strong style={{ color: '#1e40af', fontSize: '14px' }}>Preponderancia Masculina:</strong>
                    <div style={{ color: '#1e3a8a', fontSize: '13px', marginTop: '3px' }}>
                      El <strong>{porcHombres}%</strong> de las víctimas mortales corresponden al sexo masculino ({totalHombres.toLocaleString('es-MX')} casos).
                    </div>
                  </div>

                  <div style={{ backgroundColor: '#fdf2f8', borderLeft: '4px solid #ec4899', padding: '12px 14px', borderRadius: '6px' }}>
                    <strong style={{ color: '#9d174d', fontSize: '14px' }}>Víctimas del Sexo Femenino:</strong>
                    <div style={{ color: '#831843', fontSize: '13px', marginTop: '3px' }}>
                      Representan el <strong>{porcMujeres}%</strong> del total registrado ({totalMujeres.toLocaleString('es-MX')} víctimas), requiriendo especial atención bajo protocolos de perspectiva de género.
                    </div>
                  </div>

                  <div style={{ backgroundColor: '#f8fafc', borderLeft: '4px solid #8b5cf6', padding: '12px 14px', borderRadius: '6px' }}>
                    <strong style={{ color: '#5b21b6', fontSize: '14px' }}>Razón de Disparidad (H/M):</strong>
                    <div style={{ color: '#4c1d95', fontSize: '13px', marginTop: '3px' }}>
                      Por cada mujer víctima de homicidio doloso, se registran aproximadamente <strong>{ratioGenero} hombres</strong>.
                    </div>
                  </div>
                </div>
              </div>

              <div style={{ borderTop: '1px solid #f1f5f9', paddingTop: '14px', marginTop: '20px', fontSize: '12px', color: '#94a3b8' }}>
                ℹ️ Fuente de datos: Catálogos oficiales de seguridad pública (SSPC / CNS / Fiscalía General).
              </div>
            </div>
          </div>
        )}
      </div>
    );
  }
}
