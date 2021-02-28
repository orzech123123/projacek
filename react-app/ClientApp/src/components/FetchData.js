import React, { Component } from 'react';

export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor(props) {
    super(props);
      this.state = { orders: [], syncs: [], loading: true };
  }

  componentDidMount() {
    this.populateWeatherData();
  }

  static renderOrdersTable(orders, syncs) {
      return (
          <div>
              <table className='table table-striped' aria-labelledby="tabelLabel">
                  <thead>
                      <tr>
                          <th>Provider</th>
                          <th>Name</th>
                          <th>Code</th>
                          <th>Date</th>
                          <th>Actions</th>
                      </tr>
                  </thead>
                  <tbody>
                      {syncs.map(sync =>
                          <tr key={sync.providerProductId}>
                              <td>{sync.providerType}</td>
                              <td>{sync.name}</td>
                              <td>{sync.code}</td>
                              <td>{sync.date}</td>
                              <td>
                                  {sync.providerType === "Allegro" &&
                                      <a href={"https://allegro.pl/moje-allegro/sprzedaz/zamowienia/" + sync.providerOrderId }>Kliknij</a>
                                  }
                              </td>
                          </tr>
                      )}
                  </tbody>
              </table>

              <table className='table table-striped' aria-labelledby="tabelLabel">
                  <thead>
                      <tr>
                          <th>Provider</th>
                          <th>Provider Id</th>
                          <th>Name</th>
                          <th>Code</th>
                          <th>Date</th>
                      </tr>
                  </thead>
                  <tbody>
                      {orders.map(order =>
                          <tr key={order.providerProductId}>
                              <td>{order.providerType}</td>
                              <td>{order.providerOrderId}</td>
                              <td>{order.name}</td>
                              <td>{order.code}</td>
                              <td>{order.date}</td>
                          </tr>
                      )}
                  </tbody>
              </table>
          </div>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
        : FetchData.renderOrdersTable(this.state.orders, this.state.syncs);

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
      console.log(data);
      this.setState({ orders: data.orders, syncs: data.syncs, loading: false });
  }
}
