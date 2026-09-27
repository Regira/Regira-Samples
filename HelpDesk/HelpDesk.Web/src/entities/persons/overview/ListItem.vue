<template>
    <div class="row border-bottom py-2">
        <div class="col-auto">
            <!-- Row-edit affordance follows config.isComplex: a real entity (page) links to its Details route;
                 a very basic entity (modal) opens FormModalButton. Forward @remove either way so a delete from
                 inside the modal refreshes the pooled overview — without it the deleted row lingers until reload. -->
            <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
            <FormModalButton v-else v-model="item" :readonly="readonly" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        </div>

        <div class="col text-truncate">
            <span class="hd-avatar hd-avatar-sm me-1" :class="item.$isEmployee ? 'hd-avatar-staff' : 'hd-avatar-customer'">{{ (item.givenName?.[0] ?? "") + (item.familyName?.[0] ?? "") }}</span>
            <span class="fw-semibold">{{ item.$title }}</span>
            <span class="badge ms-1" :class="item.$isEmployee ? 'text-bg-primary' : 'text-bg-info'">{{ $t(item.$isEmployee ? "employee" : "customer") }}</span>
            <span v-if="!item.isActive" class="badge text-bg-secondary ms-1">{{ $t("inactive") }}</span>
        </div>
        <div class="col d-none d-md-block text-truncate small">{{ item.email }}</div>
        <div class="col d-none d-lg-block text-truncate small">
            <template v-if="item.$isEmployee">
                <SupportTeamButton v-if="item.supportTeam" :model-value="getSupportTeam(item.supportTeam)" /> {{ getSupportTeam(item.supportTeam)?.$title }}
                <span class="text-muted d-block" style="font-size: 0.75rem">{{ item.jobTitle }}</span>
            </template>
            <template v-else>{{ item.company }}</template>
        </div>
        <div class="col-1 d-none d-xl-block text-center"><i v-if="item.hasAccount" class="bi bi-person-check-fill text-success" :title="$t('hasAccount')"></i></div>
        <div class="col-2 d-none d-lg-block text-truncate">{{ formatDate(item.created) }}</div>

        <div class="col-auto">
            <!-- readonly comes from List.vue; it is the hook for permission-gating (entities.patterns.md -> Permission-gated UI) -->
            <ConfirmButton
                v-if="!readonly"
                icon="delete"
                :modal-type="ModalType.danger"
                :modal-title="$t('delete')"
                :modal-labels="{ cancel: $t('cancel'), submit: $t('delete') }"
                @confirm="$emit('request-remove', item)"
            >
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
import { FormModalButton as SupportTeamButton, useEntityStore as useSupportTeamStore } from "@/entities/support-teams"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool: getSupportTeam } = useSupportTeamStore()
</script>
