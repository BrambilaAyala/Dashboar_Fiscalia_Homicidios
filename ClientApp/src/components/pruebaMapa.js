import React, { Component } from 'react';
import { Chart, registerables } from 'chart.js';
import { ChoroplethController, GeoFeature, ProjectionScale, ColorScale } from 'chartjs-chart-geo';
import { feature } from 'topojson-client';

Chart.register(ChoroplethController, GeoFeature, ProjectionScale, ColorScale, ...registerables);

// Censo oficial de población por entidad federativa (INEGI / CONAPO)
const POBLACION_INEGI = {
  "AGUASCALIENTES": 1425607,
  "BAJA CALIFORNIA": 3769020,
  "BAJA CALIFORNIA SUR": 798447,
  "CAMPECHE": 928363,
  "COAHUILA DE ZARAGOZA": 3146771,
  "COAHUILA": 3146771,
  "COLIMA": 731391,
  "CHIAPAS": 5543828,
  "CHIHUAHUA": 3741869,
  "CIUDAD DE MÉXICO": 9209944,
  "CIUDAD DE MEXICO": 9209944,
  "DURANGO": 1832650,
  "GUANAJUATO": 6166934,
  "GUERRERO": 3540685,
  "HIDALGO": 3082841,
  "JALISCO": 8348151,
  "MÉXICO": 16992418,
  "MEXICO": 16992418,
  "ESTADO DE MÉXICO": 16992418,
  "ESTADO DE MEXICO": 16992418,
  "MICHOACÁN DE OCAMPO": 4748846,
  "MICHOACAN DE OCAMPO": 4748846,
  "MICHOACÁN": 4748846,
  "MICHOACAN": 4748846,
  "MORELOS": 1971520,
  "NAYARIT": 1235456,
  "NUEVO LEÓN": 5784442,
  "NUEVO LEON": 5784442,
  "OAXACA": 4132148,
  "PUEBLA": 6583278,
  "QUERÉTARO": 2368467,
  "QUERETARO": 2368467,
  "QUINTANA ROO": 1857985,
  "SAN LUIS POTOSÍ": 2822255,
  "SAN LUIS POTOSI": 2822255,
  "SINALOA": 3026943,
  "SONORA": 2944840,
  "TABASCO": 2402598,
  "TAMAULIPAS": 3527735,
  "TLAXCALA": 1342977,
  "VERACRUZ DE IGNACIO DE LA LLAVE": 8062579,
  "VERACRUZ": 8062579,
  "YUCATÁN": 2320898,
  "YUCATAN": 2320898,
  "ZACATECAS": 1622138
}; //Eviatar sacrlo de manera manual la poblacion del INEGI, sacarlo de un documento

export class PruebaMapa extends Component {
  state = {
    mapaHomicidios: {},
    modo: 'homicidios', // 'homicidios' o 'habitantes'
    loading: true,
    topoFeatures: null,
    estadoSeleccionado: null
  };

  chartInstance = null;

  componentDidMount() {
    this.cargarDatos();
  }

  componentWillUnmount() {
    if (this.chartInstance) {
      this.chartInstance.destroy();
      this.chartInstance = null;
    }
  }

  cargarDatos = async () => {
    try {
      // 1. Cargar TopoJSON de México
      const resGeo = await fetch('/mex.topo.json');
      if (!resGeo.ok) {
        throw new Error(`HTTP ${resGeo.status} al descargar /mex.topo.json`);
      }
      const mex = await resGeo.json();
      const features = feature(mex, mex.objects.mex).features;

      // 2. Cargar datos de homicidios desde el backend con protección de tipo de contenido
      const mapaHomicidios = {};
      try {
        const resHomicidios = await fetch('/Homicidios/totales-por-entidad');
        if (resHomicidios.ok) {
          const contentType = resHomicidios.headers.get('content-type') || '';
          if (contentType.includes('application/json')) {
            const dataHomicidios = await resHomicidios.json();
            if (Array.isArray(dataHomicidios)) {
              dataHomicidios.forEach(item => {
                const nombre = (item.entidad || '').toUpperCase().trim();
                mapaHomicidios[nombre] = item.num_Muertos || 0;
              });
            }
          }
        } else {
          // Intento con endpoint alternativo /LectorPdf
          const resAlt = await fetch('/LectorPdf');
          if (resAlt.ok) {
            const dataAlt = await resAlt.json();
            if (Array.isArray(dataAlt)) {
              dataAlt.forEach(item => {
                const nombre = (item.entidad || '').toUpperCase().trim();
                mapaHomicidios[nombre] = (mapaHomicidios[nombre] || 0) + (item.num_Muertos || 0);
              });
            }
          }
        }
      } catch (errHomicidios) {
        console.warn('Aviso: No se pudieron cargar los datos de homicidios del backend:', errHomicidios);
      }

      this.setState(
        {
          mapaHomicidios,
          topoFeatures: features,
          loading: false
        },
        () => this.renderizarMapa()
      );
    } catch (error) {
      console.error('Error al cargar datos del mapa:', error);
      this.setState({ loading: false });
    }
  };

  cambiarModo = (nuevoModo) => {
    if (this.state.modo === nuevoModo) return;
    this.setState({ modo: nuevoModo }, () => this.renderizarMapa());
  };

  obtenerPoblacion(nombreEntidad) {
    const norm = (nombreEntidad || '').toUpperCase().trim();
    if (POBLACION_INEGI[norm] !== undefined) {
      return POBLACION_INEGI[norm];
    }
    const key = Object.keys(POBLACION_INEGI).find(k => k.includes(norm) || norm.includes(k));
    return key ? POBLACION_INEGI[key] : 1000000;
  }

  obtenerHomicidios(nombreEntidad) {
    const norm = (nombreEntidad || '').toUpperCase().trim();
    const map = this.state.mapaHomicidios;
    if (map[norm] !== undefined) {
      return map[norm];
    }
    const key = Object.keys(map).find(k => k.includes(norm) || norm.includes(k));
    return key ? map[key] : 0;
  }

  renderizarMapa() {
    if (!this.chartRef || !this.state.topoFeatures) return;

    if (this.chartInstance) {
      this.chartInstance.destroy();
      this.chartInstance = null;
    }

    const { modo, topoFeatures } = this.state;
    const esHomicidios = modo === 'homicidios';

    const dataWithValues = topoFeatures.map(d => {
      const nombre = d.properties.name;
      const valor = esHomicidios ? this.obtenerHomicidios(nombre) : this.obtenerPoblacion(nombre);
      return {
        feature: d,
        value: valor
      };
    });

    const ctx = this.chartRef.getContext('2d');

    this.chartInstance = new Chart(ctx, {
      type: 'choropleth',
      data: {
        labels: topoFeatures.map(d => d.properties.name),
        datasets: [
          {
            label: esHomicidios ? 'Homicidios Dolosos' : 'Población (Habitantes)',
            outline: topoFeatures,
            data: dataWithValues
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            callbacks: {
              label: (context) => {
                const nombre = context.raw?.feature?.properties?.name || '';
                const hom = this.obtenerHomicidios(nombre);
                const pob = this.obtenerPoblacion(nombre);

                if (esHomicidios) {
                  const tasa = pob > 0 ? ((hom / pob) * 100000).toFixed(2) : '0';
                  return [
                    ` Homicidios Dolosos: ${hom.toLocaleString('es-MX')}`,
                    ` Población INEGI: ${pob.toLocaleString('es-MX')} hab.`,
                    ` Tasa: ${tasa} por cada 100k hab.`
                  ];
                } else {
                  return [
                    ` Población Total: ${pob.toLocaleString('es-MX')} habitantes`,
                    ` Homicidios Registrados: ${hom.toLocaleString('es-MX')}`
                  ];
                }
              }
            }
          }
        },
        scales: {
          projection: {
            axis: 'x',
            projection: 'mercator'
          },
          color: {
            axis: 'x',
            interpolate: esHomicidios ? 'ylOrRd' : 'blues',
            quantize: 6,
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
    const { modo, loading, mapaHomicidios } = this.state;
    const esHomicidios = modo === 'homicidios';

    // Métricas para los cuadros informativos
    const totalHomicidios = Object.values(mapaHomicidios).reduce((a, b) => a + b, 0);
    const totalPoblacionNacional = 126014024; // Censo INEGI

    // Entidad con mayor valor según modo
    let entidadTop = 'N/A';
    let valorTop = 0;

    if (esHomicidios) {
      const topEntry = Object.entries(mapaHomicidios).sort((a, b) => b[1] - a[1])[0];
      if (topEntry) {
        entidadTop = topEntry[0];
        valorTop = topEntry[1];
      }
    } else {
      entidadTop = 'ESTADO DE MÉXICO';
      valorTop = 16992418;
    }

    return (
      <div style={{ padding: '15px' }}>
        <div style={{ textAlign: 'center', marginBottom: '20px' }}>
          <h2 style={{ fontWeight: 'bold', color: '#1a3353', marginBottom: '5px' }}>
            Mapa Geográfico Interactivo de México
          </h2>
          <p style={{ color: '#666', fontSize: '15px', margin: 0 }}>
            Visualización coroplética por densidad demográfica y concentración delictiva (SSPC / INEGI)
          </p>
        </div>

        {/* SELECTOR DE MODOS */}
        <div style={{ display: 'flex', justifyContent: 'center', marginBottom: '25px', gap: '15px' }}>
          <button
            onClick={() => this.cambiarModo('homicidios')}
            style={{
              padding: '12px 24px',
              fontSize: '15px',
              fontWeight: '600',
              borderRadius: '8px',
              border: esHomicidios ? '2px solid #d32f2f' : '1px solid #ccc',
              backgroundColor: esHomicidios ? '#ffebee' : '#fff',
              color: esHomicidios ? '#c62828' : '#555',
              cursor: 'pointer',
              boxShadow: esHomicidios ? '0 4px 12px rgba(211,47,47,0.2)' : 'none',
              transition: 'all 0.3s ease'
            }}
          >
            🔥 Modo: Volumen de Homicidios Dolosos
          </button>

          <button
            onClick={() => this.cambiarModo('habitantes')}
            style={{
              padding: '12px 24px',
              fontSize: '15px',
              fontWeight: '600',
              borderRadius: '8px',
              border: !esHomicidios ? '2px solid #1976d2' : '1px solid #ccc',
              backgroundColor: !esHomicidios ? '#e3f2fd' : '#fff',
              color: !esHomicidios ? '#0d47a1' : '#555',
              cursor: 'pointer',
              boxShadow: !esHomicidios ? '0 4px 12px rgba(25,118,210,0.2)' : 'none',
              transition: 'all 0.3s ease'
            }}
          >
            👥 Modo: Volumen de Habitantes (Censo INEGI)
          </button>
        </div>

        {/* TARJETAS DE CONTEXTO */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '15px', marginBottom: '20px' }}>
          <div style={{ backgroundColor: '#fff', padding: '15px', borderRadius: '10px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', borderLeft: esHomicidios ? '5px solid #d32f2f' : '5px solid #1976d2' }}>
            <span style={{ fontSize: '13px', color: '#777', textTransform: 'uppercase', fontWeight: 'bold' }}>Métrica Activa</span>
            <div style={{ fontSize: '18px', fontWeight: 'bold', color: esHomicidios ? '#c62828' : '#0d47a1', marginTop: '4px' }}>
              {esHomicidios ? 'Conteo de Homicidios Dolosos' : 'Población Demográfica (INEGI)'}
            </div>
            <small style={{ color: '#888' }}>{esHomicidios ? 'Escala de alerta cálida (Amarillo a Rojo)' : 'Escala demográfica fría (Celeste a Azul)'}</small>
          </div>

          <div style={{ backgroundColor: '#fff', padding: '15px', borderRadius: '10px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', borderLeft: '5px solid #ff9800' }}>
            <span style={{ fontSize: '13px', color: '#777', textTransform: 'uppercase', fontWeight: 'bold' }}>Total Nacional Registrado</span>
            <div style={{ fontSize: '20px', fontWeight: 'bold', color: '#333', marginTop: '4px' }}>
              {esHomicidios ? `${totalHomicidios.toLocaleString('es-MX')} víctimas` : `${totalPoblacionNacional.toLocaleString('es-MX')} habitantes`}
            </div>
            <small style={{ color: '#888' }}>{esHomicidios ? 'Suma acumulada en base de datos' : 'Total 32 Entidades Federativas'}</small>
          </div>

          <div style={{ backgroundColor: '#fff', padding: '15px', borderRadius: '10px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', borderLeft: '5px solid #4caf50' }}>
            <span style={{ fontSize: '13px', color: '#777', textTransform: 'uppercase', fontWeight: 'bold' }}>Mayor Concentración</span>
            <div style={{ fontSize: '18px', fontWeight: 'bold', color: '#2e7d32', marginTop: '4px' }}>
              {entidadTop}
            </div>
            <small style={{ color: '#555' }}>
              {esHomicidios ? `${valorTop} homicidios reportados` : `${valorTop.toLocaleString('es-MX')} habitantes`}
            </small>
          </div>
        </div>

        {/* CONTENEDOR DEL CANVAS DEL MAPA */}
        <div style={{ backgroundColor: '#fff', borderRadius: '12px', padding: '20px', boxShadow: '0 4px 16px rgba(0,0,0,0.08)', position: 'relative' }}>
          {loading && (
            <div style={{ textAlign: 'center', padding: '50px' }}>
              <p><em>Cargando mapa y sincronizando geometrías de México...</em></p>
            </div>
          )}
          <div style={{ width: '100%', height: '550px' }}>
            <canvas ref={(ref) => (this.chartRef = ref)} id="canvasMapa"></canvas>
          </div>
          <div style={{ textAlign: 'center', marginTop: '10px' }}>
            <small style={{ color: '#888' }}>
              💡 Pasa el cursor sobre cualquier estado para ver detalles comparativos de población, homicidios y tasa por cada 100k habitantes.
            </small>
          </div>
        </div>
      </div>
    );
  }
}
