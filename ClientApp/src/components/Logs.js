import React, { Component, useState, useEffect } from 'react';
import Linkify from 'linkifyjs/react';
import moment from 'moment'
import 'moment/locale/pl'
import Table from "./Table"
import { store } from 'react-notifications-component';
import { useThrottle } from 'use-throttle';

moment.locale("pl")

export class Logs extends Component {
    render() {
        return <div>
            <LogsTable />
        </div>
    }
}

function MessageColumn({ value }) {
    const [link, setLink] = useState(null);
    const linkThrottled = useThrottle(link, 1000);

    useEffect(() => {
        if (!linkThrottled) {
            return;
        }

        fetch(linkThrottled, {
                method: 'POST'
            })
            .then(function (response) {
                return response.text();
            })
            .then(data => {
                console.log(data);
                store.addNotification({
                    title: "Komunikat z serwera",
                    message: data,
                    type: "success",
                    insert: "bottom",
                    container: "bottom-right",
                    animationIn: ["animate__animated", "animate__fadeIn"],
                    animationOut: ["animate__animated", "animate__fadeOut"],
                    dismiss: {
                        duration: 3000,
                        onScreen: true
                    }
                });
            });
    }, [linkThrottled]);

    let linkProps = {
        onClick: (event) => {
            let domain = document.domain;
            let href = event.target.getAttribute('href');

            if (!href || !href.includes(domain)) {
                return;
            }

            event.preventDefault();

            setLink(prevLink => prevLink == href ? prevLink + " " : href);
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
