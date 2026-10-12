import React, { Component } from 'react';
import { Link } from 'react-router-dom';

export class Home extends Component {
  static displayName = Home.name;

  render() {
    const modulos = [
      {
        titulo: 'Mapa Geográfico Nacional',
        ruta: '/Map',
        icono: '🗺️',
        colorBorde: '#3b82f6',
        descripcion: 'Visualización coroplética interactiva de las 32 entidades federativas. Alterna entre volumen delictivo y censo demográfico oficial del INEGI.',
        etiqueta: 'Exploración Geoespacial'
      },
      {
        titulo: 'Analítica y Series de Tiempo',
        ruta: '/Graphs',
        icono: '📊',
        colorBorde: '#ef4444',
        descripcion: 'Comparativas multivariables, análisis de estacionalidad por trimestre y patrones de género (hombres, mujeres y no identificados).',
        etiqueta: 'Tendencias y Género'
      },
      {
        titulo: 'Centro de Inteligencia (Fisca)',
        ruta: '/Fisca',
        icono: '📋',
        colorBorde: '#10b981',
        descripcion: 'Registro forense detallado a nivel municipal con filtros dinámicos, fuentes periodísticas de verificación y exportador de datos a CSV.',
        etiqueta: 'Registros Forenses'
      },
      {
        titulo: 'Consolidado de Casos',
        ruta: '/Homicidios',
        icono: '📈',
        colorBorde: '#f59e0b',
        descripcion: 'Reporte ejecutivo tabular y gráfico directo con los incidentes de mayor impacto registrados en el sistema.',
        etiqueta: 'Conteo General'
      }
    ];

    const integrantes = [
      'Leonardo Brambila Ayala',
      'Aldo Rafael Guzmán Villanueva',
      'Francisco Javier Atondo Nubes',
      'Manuel Alejandro Heredia Nogales'
    ];

    return (
      <div style={{ padding: '10px 15px', maxWidth: '1200px', margin: '0 auto' }}>
        {/* HERO HEADER */}
        <div
          style={{
            background: 'linear-gradient(135deg, #1e293b 0%, #0f172a 100%)',
            color: '#fff',
            padding: '40px 30px',
            borderRadius: '16px',
            marginBottom: '35px',
            boxShadow: '0 10px 25px rgba(15,23,42,0.15)',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            flexWrap: 'wrap',
            gap: '20px'
          }}
        >
          <div style={{ flex: '1 1 500px' }}>
            <span
              style={{
                backgroundColor: 'rgba(239,68,68,0.2)',
                color: '#f87171',
                padding: '5px 12px',
                borderRadius: '20px',
                fontSize: '13px',
                fontWeight: 'bold',
                textTransform: 'uppercase',
                letterSpacing: '1px'
              }}
            >
              SSPC • SAT • Fiscalía General • INEGI
            </span>
            <h1 style={{ fontSize: '32px', fontWeight: 'bold', margin: '15px 0 10px 0', lineHeight: 1.2 }}>
              Sistema de Monitoreo Forense e Inteligencia de Homicidios Dolosos
            </h1>
            <p style={{ color: '#94a3b8', fontSize: '16px', maxWidth: '650px', lineHeight: 1.5, margin: 0 }}>
              Plataforma tecnológica orientada al análisis criminológico, detección de patrones de violencia y recopilación de reportes diarios oficiales del Gobierno de México.
            </p>
          </div>

          <div style={{ display: 'flex', gap: '20px', alignItems: 'center', backgroundColor: 'rgba(255,255,255,0.05)', padding: '15px 25px', borderRadius: '12px' }}>
            <img src="/Unison.png" alt="Universidad de Sonora" style={{ height: '70px', objectFit: 'contain' }} />
            <div style={{ width: '1px', height: '50px', backgroundColor: 'rgba(255,255,255,0.2)' }} />
            <img src="/gob.png" alt="Gobierno de México" style={{ height: '50px', objectFit: 'contain' }} />
          </div>
        </div>

        {/* MÓDULOS DEL SISTEMA */}
        <h2 style={{ fontSize: '22px', fontWeight: 'bold', color: '#1e293b', marginBottom: '20px' }}>
          Módulos de Análisis y Visualización
        </h2>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))', gap: '20px', marginBottom: '40px' }}>
          {modulos.map((mod, idx) => (
            <a
              key={idx}
              href={mod.ruta}
              style={{
                textDecoration: 'none',
                color: 'inherit',
                backgroundColor: '#fff',
                padding: '25px 20px',
                borderRadius: '14px',
                boxShadow: '0 2px 10px rgba(0,0,0,0.05)',
                borderTop: `4px solid ${mod.colorBorde}`,
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'space-between',
                transition: 'transform 0.2s ease, box-shadow 0.2s ease'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'translateY(-4px)';
                e.currentTarget.style.boxShadow = '0 8px 20px rgba(0,0,0,0.09)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'translateY(0)';
                e.currentTarget.style.boxShadow = '0 2px 10px rgba(0,0,0,0.05)';
              }}
            >
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '15px' }}>
                  <span style={{ fontSize: '32px' }}>{mod.icono}</span>
                  <span style={{ fontSize: '11px', fontWeight: 'bold', color: '#64748b', backgroundColor: '#f1f5f9', padding: '3px 8px', borderRadius: '8px' }}>
                    {mod.etiqueta}
                  </span>
                </div>
                <h3 style={{ fontSize: '18px', fontWeight: 'bold', color: '#0f172a', marginBottom: '10px' }}>
                  {mod.titulo}
                </h3>
                <p style={{ color: '#64748b', fontSize: '14px', lineHeight: 1.4, margin: 0 }}>
                  {mod.descripcion}
                </p>
              </div>

              <div style={{ marginTop: '20px', color: mod.colorBorde, fontWeight: 'bold', fontSize: '14px', display: 'flex', alignItems: 'center', gap: '6px' }}>
                Ingresar al módulo ➔
              </div>
            </a>
          ))}
        </div>

        {/* ARQUITECTURA Y FUENTE OFICIAL */}
        <div style={{ backgroundColor: '#fff', padding: '25px', borderRadius: '14px', boxShadow: '0 2px 10px rgba(0,0,0,0.05)', marginBottom: '35px' }}>
          <h3 style={{ fontSize: '18px', fontWeight: 'bold', color: '#1e293b', marginBottom: '15px' }}>
            ⚙️ Arquitectura del Pipeline de Inteligencia (100% C# .NET)
          </h3>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '15px', textAlign: 'center' }}>
            <div style={{ backgroundColor: '#f8fafc', padding: '15px', borderRadius: '10px' }}>
              <div style={{ fontSize: '24px', marginBottom: '6px' }}>🌐</div>
              <strong>1. Ingesta Directa</strong>
              <p style={{ fontSize: '13px', color: '#64748b', margin: '4px 0 0 0' }}>Descarga automática vía HTTP de boletines diarios de la CNS / SSPC.</p>
            </div>
            <div style={{ backgroundColor: '#f8fafc', padding: '15px', borderRadius: '10px' }}>
              <div style={{ fontSize: '24px', marginBottom: '6px' }}>🧠</div>
              <strong>2. Inferencia Forense</strong>
              <p style={{ fontSize: '13px', color: '#64748b', margin: '4px 0 0 0' }}>Normalizador difuso (Jaro-Winkler) y deducción inversa por municipio.</p>
            </div>
            <div style={{ backgroundColor: '#f8fafc', padding: '15px', borderRadius: '10px' }}>
              <div style={{ fontSize: '24px', marginBottom: '6px' }}>🗄️</div>
              <strong>3. Persistencia Relacional</strong>
              <p style={{ fontSize: '13px', color: '#64748b', margin: '4px 0 0 0' }}>Almacenamiento seguro en MySQL con integridad referencial INEGI.</p>
            </div>
            <div style={{ backgroundColor: '#f8fafc', padding: '15px', borderRadius: '10px' }}>
              <div style={{ fontSize: '24px', marginBottom: '6px' }}>📊</div>
              <strong>4. Analítica Ejecutiva</strong>
              <p style={{ fontSize: '13px', color: '#64748b', margin: '4px 0 0 0' }}>Visualización reactiva en tiempo real con React, Recharts y TopoJSON.</p>
            </div>
          </div>
        </div>

        {/* FICHA ACADÉMICA */}
        <div
          style={{
            backgroundColor: '#f1f5f9',
            padding: '25px',
            borderRadius: '14px',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            flexWrap: 'wrap',
            gap: '20px'
          }}
        >
          <div>
            <h4 style={{ margin: 0, color: '#0f172a', fontWeight: 'bold' }}>Universidad de Sonora (UNISON)</h4>
            <p style={{ margin: '4px 0 0 0', color: '#475569', fontSize: '14px' }}>
              Licenciatura en Ciencias de la Computación • Clase: Análisis y Diseño Orientado a Objetos (2024-2)
            </p>
          </div>

          <div>
            <span style={{ fontSize: '12px', fontWeight: 'bold', color: '#64748b', textTransform: 'uppercase' }}>Equipo de Desarrollo:</span>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px', marginTop: '6px' }}>
              {integrantes.map((nombre, i) => (
                <span
                  key={i}
                  style={{
                    backgroundColor: '#fff',
                    padding: '4px 10px',
                    borderRadius: '8px',
                    fontSize: '13px',
                    color: '#334155',
                    boxShadow: '0 1px 3px rgba(0,0,0,0.05)',
                    fontWeight: '500'
                  }}
                >
                  👤 {nombre}
                </span>
              ))}
            </div>
          </div>
        </div>
      </div>
    );
  }
}