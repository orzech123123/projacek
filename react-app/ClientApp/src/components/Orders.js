import React, { Component, useState } from 'react';

import Table from "./Table"

export class Orders extends Component {

  render() {
      return <OrdersTable />
  }
}

function OrdersTable() {
    const [data, setData] = useState([]);
    const [loading, setLoading] = React.useState(false);
    const [pageCount, setPageCount] = React.useState(0);
    const fetchIdRef = React.useRef(0);
    const sortIdRef = React.useRef(0);

    const [filter, setFilter] = useState('');

    const columns = React.useMemo(
        () => [
            {
                Header: "Nadano przez",
                accessor: "providerType"
            },
            {
                Header: "Nazwa towaru - aukcji",
                accessor: "name"
            },
            {
                Header: "Kod",
                accessor: "code"
            },
            {
                Header: "Data",
                accessor: "date"
            },
            {
                Header: 'Akcje',
                id: 'actions',
                disableSortBy: true,
                accessor: d => d.providerOrderId,
                Cell: props => {
                    if (props.row.original.providerType === "Allegro") {
                        return <a target="_blank" href={"https://allegro.pl/moje-allegro/sprzedaz/zamowienia/" + props.value}>Zamówienie</a>
                    }

                    if (props.row.original.providerType === "Apaczka") {
                        return <a target="_blank" href={"https://panel.apaczka.pl/zlecenia/" + props.value}>Zamówienie</a>
                    }

                    return (null);
                }
            }

        ],
        []
    );

    const fetchData = React.useCallback(({ pageSize, pageIndex, sortBy, filter }) => {
        const fetchId = ++fetchIdRef.current;

        setLoading(true);

        var orderBy = sortBy.length > 0 ? sortBy[0].id : null;
        var orderByParam = !orderBy || orderBy == "null" ? "" : "&orderBy=" + orderBy;
        var isDescending = sortBy.length > 0 ? sortBy[0].desc : true;

        fetch('/orders?isDescending=' + isDescending + orderByParam + "&pageSize=" + pageSize + "&pageIndex=" + pageIndex + "&filter=" + filter)
            .then(function (response) {
                return response.json();
            })
            .then(data => {
                if (fetchId === fetchIdRef.current) {
                    setData(data.syncs);

                    setPageCount(Math.ceil(data.count / pageSize));

                    setLoading(false);
                }
            });
    }, []);

    return (
        <div>
            <input
                type="text"
                value={filter}
                placeholder="Wyszukaj"
                onChange={e => setFilter(e.target.value)}
            />

            <Table
                columns={columns}
                filter={filter}
                data={data}
                fetchData={fetchData}
                loading={loading}
                pageCount={pageCount}
            />
        </div>
    );
}
