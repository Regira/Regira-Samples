import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

const api = "/priorities"

const config: IConfig = {
    id: Entity.name,
    key: "Priority",
    isComplex: true,

    routePrefix: "priorities",
    baseQueryParams: {},
    initialQuery: {},

    overviewTitle: "priorities",
    detailsTitle: "priority",
    description: "priority.description",
    icon: "bi bi-exclamation-triangle",

    defaultPageSize: 15,

    api,
    searchUrl: api + "/search",
    saveUrl: api,
}

export default config
