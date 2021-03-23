import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
import ReactNotification from 'react-notifications-component'

import 'react-notifications-component/dist/theme.css'

export class Layout extends Component {
  static displayName = Layout.name;

  render () {
    return (
      <div>
        <ReactNotification />
        <NavMenu />
        <Container>
          {this.props.children}
        </Container>
      </div>
    );
  }
}
