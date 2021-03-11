import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Orders } from './components/Orders';
import { Counter } from './components/Counter';
import { Logs } from './components/Logs';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Orders} />
            <Route path='/orders-list' component={Orders} />
            <Route path='/logs-list' component={Logs} />
      </Layout>
    );
  }
}
