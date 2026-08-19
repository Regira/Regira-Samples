import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

const api = "/statuses"

const config: IConfig = {
    id: Entity.name,
    key: "Status",
    isComplex: true,

    routePrefix: "statuses",
    baseQueryParams: {},
    initialQuery: {},

    overviewTitle: "statuses",
    detailsTitle: "status",
    description: "status.description",
    icon: "bi bi-signpost-split",

    defaultPageSize: 15,

    api,
    searchUrl: api + "/search",
    saveUrl: api,
}

export default config
