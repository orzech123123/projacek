import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

export class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor (props) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
        collapsed: true,
        allegroSettings: { }
    };
   }

   componentDidMount() {
       this.fetchSettings();
   }

  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render () {
    return (
      <header>
        <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
                <Container>
                   {
                    <NavbarBrand tag={Link} to="/"><img style={{ width: 200 + "px" }} src="/logo.jpg" /></NavbarBrand>}
            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
            <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
              <ul className="navbar-nav flex-grow">
                    {<NavItem>
                        <NavLink tag={Link} className="text-dark" to="/orders-list">Wydania</NavLink>
                    </NavItem>}
                    {<NavItem>
                        <NavLink tag={Link} className="text-dark" to="/logs-list">Logi</NavLink>
                    </NavItem>}
                <NavItem>
                    <a href={"https://allegro.pl/auth/oauth/authorize?response_type=code&client_id=" + this.state.allegroSettings.clientId + "&redirect_uri=" + this.state.allegroSettings.returnUrl } className="text-dark nav-link">Zaloguj do Allegro</a>
                </NavItem>
              </ul>
            </Collapse>
          </Container>
        </Navbar>
      </header>
    );
    }


    async fetchSettings() {
        const response = await fetch('allegro/settings');
        const data = await response.json();
        this.setState({ collapsed: this.state.collapsed, allegroSettings: data });
    }
}
