import React, { Component } from 'react';
import { Collapse, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

export class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor (props) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true
    };
  }

  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render() {
    return (
      <header>
        <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" container light>
          <NavbarBrand tag={Link} to="/">ProyectoFisca</NavbarBrand>
          <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
          <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
            <ul className="navbar-nav flex-grow">
              <NavItem>
                <NavLink className="text-dark" href="/">Home</NavLink>
              </NavItem>
              <NavItem>
                <NavLink className='text-dark' href='/Fisca'>Fisca</NavLink>
              </NavItem>
              <NavItem>
                <NavLink className='text-dark' href='/Graphs'>Graphs</NavLink>
              </NavItem>
              <NavItem>
                <NavLink className='text-dark' href='/Map'>Mapa</NavLink>
              </NavItem>
              <NavItem>
                <NavLink className='text-dark' href='/Homicidios'>Homicidios</NavLink>
              </NavItem>
            </ul>
          </Collapse>
        </Navbar>
      </header>
    );
  }
}
