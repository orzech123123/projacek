﻿import React, { useState, useEffect, useCallback } from "react";
import { useTable, useSortBy, usePagination } from "react-table";

import "../styles/MegaTable.css";

function Table({
    columns,
    data,
    onSort,
    fetchData,
    loading,
    pageCount: controlledPageCount
}) {
    const {
        getTableProps,
        getTableBodyProps,
        headerGroups,
        prepareRow,
        page,
        canPreviousPage,
        canNextPage,
        pageOptions,
        gotoPage,
        nextPage,
        pageCount,
        previousPage,
        setPageSize,
        state: { pageIndex, pageSize, sortBy }
    } = useTable(
        {
            columns,
            data,
            manualPagination: true,
            manualSortBy: true,
            autoResetPage: false,
            autoResetSortBy: false,
            pageCount: controlledPageCount
        },
        useSortBy,
        usePagination
    );

    useEffect(() => {
        fetchData({ pageIndex, pageSize, sortBy });
    }, [sortBy, fetchData, pageIndex, pageSize]);

    return (
        <>
            <table {...getTableProps()}>
                <thead>
                    {headerGroups.map((headerGroup) => (
                        <tr {...headerGroup.getHeaderGroupProps()}>
                            {headerGroup.headers.map((column) => (
                                // Add the sorting props to control sorting. For this example
                                // we can add them into the header props
                                <th {...column.getHeaderProps(column.getSortByToggleProps())}>
                                    {column.render("Header")}
                                    {/* Add a sort direction indicator */}
                                    <span>
                                        {column.isSorted
                                            ? column.isSortedDesc
                                                ? " 🔽"
                                                : " 🔼"
                                            : ""}
                                    </span>
                                </th>
                            ))}
                        </tr>
                    ))}
                </thead>
                <tbody {...getTableBodyProps()}>
                    {page.map((row, i) => {
                        prepareRow(row);
                        return (
                            <tr {...row.getRowProps()}>
                                {row.cells.map((cell) => {
                                    return (
                                        <td {...cell.getCellProps()}>{cell.render("Cell")}</td>
                                    );
                                })}
                            </tr>
                        );
                    })}
                    <tr>
                        {loading ? (
                            // Use our custom loading state to show a loading indicator
                            <td colSpan="10000">Loading...</td>
                        ) : (
                                <td colSpan="10000">
                                    Showing {page.length} of ~{controlledPageCount * pageSize}{" "}
                results
                                </td>
                            )}
                    </tr>
                </tbody>
            </table>
            <div className="pagination">
                <button onClick={() => gotoPage(0)} disabled={!canPreviousPage}>
                    {"<<"}
                </button>{" "}
                <button onClick={() => previousPage()} disabled={!canPreviousPage}>
                    {"<"}
                </button>{" "}
                <button onClick={() => nextPage()} disabled={!canNextPage}>
                    {">"}
                </button>{" "}
                <button onClick={() => gotoPage(pageCount - 1)} disabled={!canNextPage}>
                    {">>"}
                </button>{" "}
                <span>
                    Page{" "}
                    <strong>
                        {pageIndex + 1} of {pageOptions.length}
                    </strong>{" "}
                </span>
                <span>
                    | Go to page:{" "}
                    <input
                        type="number"
                        defaultValue={pageIndex + 1}
                        onChange={(e) => {
                            const page = e.target.value ? Number(e.target.value) - 1 : 0;
                            gotoPage(page);
                        }}
                        style={{ width: "100px" }}
                    />
                </span>{" "}
                <select
                    value={pageSize}
                    onChange={(e) => {
                        setPageSize(Number(e.target.value));
                    }}
                >
                    {[10, 20, 30, 40, 50].map((pageSize) => (
                        <option key={pageSize} value={pageSize}>
                            Show {pageSize}
                        </option>
                    ))}
                </select>
            </div>
        </>
    );
}

function MegaTable() {
    const [data, setData] = useState([]);
    const [loading, setLoading] = React.useState(false);
    const [pageCount, setPageCount] = React.useState(0);
    const fetchIdRef = React.useRef(0);
    const sortIdRef = React.useRef(0);

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
                    return (null);
                }
            }
         
        ],
        []
    );

    const fetchData = React.useCallback(({ pageSize, pageIndex, sortBy }) => {
        const fetchId = ++fetchIdRef.current;

        setLoading(true);

        var orderBy = sortBy.length > 0 ? sortBy[0].id : null;
        var orderByParam = !orderBy || orderBy == "null" ? "" : "&orderBy=" + orderBy;
        var isDescending = sortBy.length > 0 ? sortBy[0].desc : true;

        fetch('/orders?isDescending=' + isDescending + orderByParam + "&pageSize=" + pageSize + "&pageIndex=" + pageIndex)
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
        <Table
            columns={columns}
            data={data}
            fetchData={fetchData}
            loading={loading}
            pageCount={pageCount}
        />
    );
}

export default MegaTable;