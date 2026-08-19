import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

// Relative to the axios baseURL, and must equal the server's [Route(...)] exactly — repeating the base here
// sends requests to /api/api/... See entities.setup → The URL contract.
const api = "/asset-statuses"

const config: IConfig = {
    id: Entity.name,
    key: "AssetStatus",
    isComplex: true,

    routePrefix: "asset-statuses",
    baseQueryParams: {},
    initialQuery: {},

    overviewTitle: "assetStatuses",
    detailsTitle: "assetStatus",
    description: "assetStatus.description",
    icon: "bi bi-flag",

    defaultPageSize: 10,

    api,
    searchUrl: api + "/search",
    saveUrl: api,
}

export default config
