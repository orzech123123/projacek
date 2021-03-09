import React, { Component, useState } from 'react';

import Table from "./Table"

export class Logs extends Component {

  render() {
      return <LogsTable />
  }
}

function LogsTable() {
    const [data, setData] = useState([]);
    const [loading, setLoading] = React.useState(false);
    const [pageCount, setPageCount] = React.useState(0);
    const fetchIdRef = React.useRef(0);
    const sortIdRef = React.useRef(0);

    const [filter, setFilter] = useState('');

    const columns = React.useMemo(
        () => [
            {
                Header: "Data",
                accessor: "timeStamp"
            },
            {
                Header: "Poziom",
                accessor: "level"
            },
            {
                Header: "Wiadomość",
                accessor: "message"
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

        fetch('/log?isDescending=' + isDescending + orderByParam + "&pageSize=" + pageSize + "&pageIndex=" + pageIndex + "&filter=" + filter)
            .then(function (response) {
                return response.json();
            })
            .then(data => {
                if (fetchId === fetchIdRef.current) {
                    setData(data.logs);

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
