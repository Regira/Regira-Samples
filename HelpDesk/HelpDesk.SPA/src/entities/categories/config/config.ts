import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

const api = "/categories"

const config: IConfig = {
    id: Entity.name,
    key: "Category",
    isComplex: true,

    routePrefix: "categories",
    baseQueryParams: {},
    initialQuery: {},

    overviewTitle: "categories",
    detailsTitle: "category",
    description: "category.description",
    icon: "bi bi-tags",

    defaultPageSize: 15,

    api,
    searchUrl: api + "/search",
    saveUrl: api,
}

export default config
