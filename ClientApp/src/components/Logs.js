import React, { Component, useState } from 'react';
import Linkify from 'linkifyjs/react';
import moment from 'moment'
import 'moment/locale/pl'
import Table from "./Table"

moment.locale("pl")

export class Logs extends Component {

  render() {
      return <LogsTable />
  }
}

function MessageColumn({ value }) {
    let linkProps = {
        onClick: (event) => {
            console.log(event.target);
            event.preventDefault();
        }
    };

    return <Linkify options={{ attributes: linkProps }}>{value}</Linkify>
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
                id: "id",
                Header: "Data",
                accessor: d => {
                    return moment(d.timeStamp)
                        .local()
                        .format("DD MMM yyyy HH:mm:ss")
                }
            },
            {
                Header: "Poziom",
                accessor: "level"
            },
            {
                Header: "Wiadomość",
                accessor: "message",
                Cell: MessageColumn
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