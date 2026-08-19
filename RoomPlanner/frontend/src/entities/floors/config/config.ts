import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

// Relative to the axios baseURL, and must equal the server's [Route(...)] exactly — repeating the base here
// sends requests to /api/api/... See entities.setup → The URL contract.
const api = "/floors"

const config: IConfig = {
    id: Entity.name,
    key: "Floor",
    isComplex: true, // carries a relation to Building - use the Details page

    routePrefix: "floors",
    baseQueryParams: {},
    initialQuery: {},

    overviewTitle: "floors",
    detailsTitle: "floor",
    description: "floor.description",
    icon: "bi bi-layers",

    defaultPageSize: 10, // initial overview page size (raise it to show more per page, up to the server's MaxPageSize)

    api, // every *Url below defaults to `api` when omitted; keep only the ones you override
    searchUrl: api + "/search", // counted search endpoint — the overview pages through it (every controller exposes /search)
    saveUrl: api, // resource base — update/remove append /{$id} themselves
}

export default config
