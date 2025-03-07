import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import { Fisca } from "./components/Fisca";
import { FiscaOcr } from "./components/FiscaOcr";
import { Home } from "./components/Home";
import { PruebaDash } from "./components/PruebaDash";
import { Graphs } from "./components/Graphs";
//import Graphs from './components/Graphs';
import { PruebaMapa } from "./components/pruebaMapa";
import  Homicidios  from './components/Homicidios';

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/counter',
    element: <Counter />
  },
  {
    path: '/fetch-data',
    element: <FetchData />
  },
  {
    path: '/fisca',
    element: <Fisca />
  },
  {
    path: '/fiscaocr',
    element: <FiscaOcr />
  },
  {
    path: '/pruebadash',
    element: <PruebaDash />
  },
  {
    path: '/graphs',
    element: <Graphs />
  },
  {
    path: '/map',
    element: <PruebaMapa />
  },
  {
    path: "/homicidios",
    element: <Homicidios />
  }
];

export default AppRoutes;
