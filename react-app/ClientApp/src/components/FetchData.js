import React, { Component } from 'react';

export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor(props) {
    super(props);
      this.state = { orders: [], loading: true };
  }

  componentDidMount() {
    this.populateWeatherData();
  }

  static renderOrdersTable(orders) {
    return (
      <table className='table table-striped' aria-labelledby="tabelLabel">
        <thead>
          <tr>
            <th>Type</th>
            <th>Name</th>
            <th>Code</th>
            <th>Date</th>
          </tr>
        </thead>
        <tbody>
          {orders.map(order =>
            <tr key={order.date}>
              <td>{order.type}</td>
              <td>{order.name}</td>
              <td>{order.code}</td>
              <td>{order.date}</td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : FetchData.renderOrdersTable(this.state.orders);

    return (
      <div>
        <h1 id="tabelLabel" >Orders</h1>
        <p>This component demonstrates fetching Jacek's apaczka orders from the server.</p>
        {contents}
      </div>
    );
  }

  async populateWeatherData() {
    const response = await fetch('orders');
    const data = await response.json();
      this.setState({ orders: data, loading: false });
  }
}
