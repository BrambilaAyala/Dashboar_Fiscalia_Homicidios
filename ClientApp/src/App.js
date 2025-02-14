// import React, { Component } from 'react';
// import { Route, Routes } from 'react-router-dom';
// import AppRoutes from './AppRoutes';
// import { Layout } from './components/Layout';
// import './custom.css';

// export default class App extends Component {
//   static displayName = App.name;

//   render() {
//     return (
//       <Layout>
//         <Routes>
//           {AppRoutes.map((route, index) => {
//             const { element, ...rest } = route;
//             return <Route key={index} {...rest} element={element} />;
//           })}
//         </Routes>
//       </Layout>
//     );
//   }
// }
import React, { useState } from 'react';
import { Route, Routes, Link } from 'react-router-dom';
import AppRoutes from './AppRoutes';
import { DesktopOutlined, FileOutlined, PieChartOutlined, TeamOutlined, UserOutlined, DotChartOutlined } from '@ant-design/icons';
import { Breadcrumb, Layout, Menu, theme, ConfigProvider } from 'antd';
import './custom.css';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
// Registrar los componentes de Chart.js
ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
);

const { Header, Content, Footer, Sider } = Layout;

function getItem(label, key, icon, children, path) {
  return {
    key,
    icon,
    children,
    label: <Link to={path || '#'} style={{textDecoration: 'none'}}>{label}</Link>,
  };
}

const items = [
  getItem('Home', '1', <PieChartOutlined />, null, '/'),
  // getItem('Counter', '2', <DesktopOutlined />, null, '/counter'),
  // getItem('Fetch Data', '3', <FileOutlined />, null, '/fetch-data'),
  getItem('Fisca', '4', <FileOutlined />, null, '/fisca'),  
  getItem('FiscaOcr', '5', <FileOutlined />, null, '/fiscaocr' ),
  getItem('PruebaDash', '6', <PieChartOutlined />, null, '/pruebadash' ),
  getItem('Graphs', '7', <PieChartOutlined/>, null, '/graphs'),
  getItem('Mapa', '8', <DotChartOutlined />, null, '/map'),
  getItem('Homicidios', '9', <DotChartOutlined />, null, '/homicidios'),
  
];

const App = () => {
  const [collapsed, setCollapsed] = useState(false);
  const {
    token: { colorBgContainer, borderRadiusLG },
  } = theme.useToken();

  return (
    <ConfigProvider
    theme={{
      components: {
        Layout:{
          lightSiderBg: '#8b0000',
          //triggerBg: '#8b0000',
          lightTriggerColor: '#8b0000',
          SiderBg: '#8b0000',
          
        }
      },
    }}
  >
    <Layout style={{ minHeight: '100vh', SiderBg: 'red' }}>
      <Sider collapsible collapsed={collapsed} onCollapse={(value) => setCollapsed(value)}>
        <div className="demo-logo-vertical" />
        <Menu theme="dark" defaultSelectedKeys={['1']} mode="inline" items={items} />
      </Sider>
      <Layout >
        <Header style={{ padding: 0, background: colorBgContainer }} />
        <Content style={{ margin: '0 16px' }}>
          <Breadcrumb style={{ margin: '16px 0' }}>
            {/* <Breadcrumb.Item>User</Breadcrumb.Item> */}
          </Breadcrumb>
          <div
            style={{
              padding: 24,
              minHeight: 360,
              background: colorBgContainer,
              borderRadius: borderRadiusLG,
            }}
          >
            {/* Rutas de la aplicación */}
            <Routes>
              {AppRoutes.map((route, index) => {
                const { element, ...rest } = route;
                return <Route key={index} {...rest} element={element} />;
              })}
            </Routes>
          </div>
        </Content>
        <Footer style={{ textAlign: 'center' }}>
          Ant Design ©{new Date().getFullYear()} Created by Ant UED
        </Footer>
      </Layout>
    </Layout>
    </ConfigProvider>
  );
};

export default App;
