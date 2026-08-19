<template>
    <div class="row border-bottom py-2">
        <div class="col-auto">
            <!-- Row-edit affordance follows config.isComplex: a real entity (page) links to its Details route;
                 a very basic entity (modal) opens FormModalButton. Forward @remove either way so a delete from
                 inside the modal refreshes the pooled overview — without it the deleted row lingers until reload. -->
            <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
            <FormModalButton v-else v-model="item" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        </div>

        <div class="col d-none d-md-block text-truncate">
            <AssetButton :model-value="getAsset(item.asset)" /> {{ getAsset(item.asset)?.$title }}
        </div>
        <div class="col text-truncate">
            <EmployeeButton :model-value="getEmployee(item.employee)" /> {{ getEmployee(item.employee)?.$title }}
        </div>
        <div class="col-2 d-none d-lg-block text-truncate">{{ formatDate(item.assignedDate) }}</div>
        <div class="col-2 d-none d-lg-block text-truncate">
            <span v-if="item.returnedDate">{{ formatDate(item.returnedDate) }}</span>
            <span v-else class="badge text-bg-success">{{ $t("assetAssignment.active") }}</span>
        </div>

        <div class="col-auto">
            <ConfirmButton icon="delete" :modal-type="ModalType.danger" @confirm="$emit('request-remove', item)">
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
        </div>
    </div>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import { formatDate } from "@regira/modules/vue/formatters"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import FormModalButton from "../details/FormModalButton.vue"
import { FormModalButton as AssetButton, useEntityStore as useAssetStore } from "@/entities/assets"
import { FormModalButton as EmployeeButton, useEntityStore as useEmployeeStore } from "@/entities/employees"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool: getAsset } = useAssetStore()
const { fromPool: getEmployee } = useEmployeeStore()
</script>
