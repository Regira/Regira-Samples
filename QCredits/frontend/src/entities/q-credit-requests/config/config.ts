import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

const api = "/qcredit-requests"

const config: IConfig = {
    id: Entity.name,
    key: "QCreditRequest",
    isComplex: true,

    routePrefix: "qcredit-requests",
    baseQueryParams: {}, // Items is a gated collection but Details() always eager-loads it server-side; no need on List/Search
    initialQuery: {},

    overviewTitle: "qCreditRequests",
    detailsTitle: "qCreditRequest",
    description: "qCreditRequest.description",
    icon: "bi bi-wallet2",

    defaultPageSize: 15,

    api,
    searchUrl: api + "/search",
    saveUrl: api
}

export default config
