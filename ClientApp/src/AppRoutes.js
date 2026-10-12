import React, { useEffect } from 'react';
import { Home } from "./components/Home";

const RedirectTo = ({ url }) => {
  useEffect(() => {
    window.location.href = url;
  }, [url]);
  return (
    <div style={{ padding: '40px', textAlign: 'center', fontFamily: 'sans-serif' }}>
      <div className="spinner-border text-primary" role="status">
        <span className="visually-hidden">Cargando...</span>
      </div>
      <p style={{ marginTop: '15px', color: '#64748b' }}>Cargando vista oficial...</p>
    </div>
  );
};

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/fisca',
    element: <RedirectTo url="/Fisca" />
  },
  {
    path: '/graphs',
    element: <RedirectTo url="/Graphs" />
  },
  {
    path: '/map',
    element: <RedirectTo url="/Map" />
  },
  {
    path: '/homicidios',
    element: <RedirectTo url="/Homicidios" />
  }
];

export default AppRoutes;
