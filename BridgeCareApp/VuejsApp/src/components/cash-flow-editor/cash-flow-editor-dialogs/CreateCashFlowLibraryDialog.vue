<template>
    <v-dialog v-model="dialogData.showDialog" persistent max-width="450px">
        <v-card>
            <v-card-title>
                <v-layout justify-center>
                    <h3>New Cash Flow Library</h3>
                </v-layout>
            </v-card-title>
            <v-card-text>
                <v-layout column>
                    <v-text-field label="Name" v-model="createdCashFlowLibrary.name" outline></v-text-field>

                    <v-textarea rows="3" no-resize outline label="Description" v-model="createdCashFlowLibrary.description">
                    </v-textarea>
                </v-layout>
            </v-card-text>
            <v-card-actions>
                <v-layout justify-space-between row>
                    <v-btn class="ara-blue-bg white--text" @click="onSubmit(true)"
                           :disabled="createdCashFlowLibrary.name === ''">
                        Submit
                    </v-btn>
                    <v-btn class="ara-orange-bg white--text" @click="onSubmit(false)">
                        Cancel
                    </v-btn>
                </v-layout>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script lang="ts">
    import Vue from 'vue';
    import Component from 'vue-class-component';
    import {Prop, Watch} from 'vue-property-decorator';
    import {CreateCashFlowLibraryDialogData} from '@/shared/models/modals/create-cash-flow-library-dialog-data';
    import {
        CashFlowLibrary,
        emptyCashFlowLibrary,
        SplitTreatment,
        SplitTreatmentLimit
    } from '@/shared/models/iAM/cash-flow';
    import {clone} from 'ramda';
    import {hasValue} from '@/shared/utils/has-value-util';
    const ObjectID = require('bson-objectid');

    @Component
    export default class CreateCashFlowLibraryDialog extends Vue {
        @Prop() dialogData: CreateCashFlowLibraryDialogData;

        createdCashFlowLibrary: CashFlowLibrary = clone({...emptyCashFlowLibrary, id: ObjectID.generate()});

        /**
         * Sets createdCashFlowLibrary class property using dialogData class property
         */
        @Watch('dialogData')
        onDialogDataChanged() {
            this.createdCashFlowLibrary = clone({
                ...this.createdCashFlowLibrary,
                splitTreatments: this.dialogData.splitTreatments
            });
        }

        /**
         * Emits a new cash flow library object or null to the parent component and resets createdCashFlowLibrary
         * class property
         * @param submit Boolean
         */
        onSubmit(submit: boolean) {
            if (hasValue(this.createdCashFlowLibrary.splitTreatments)) {
                this.setIdsForNewLibrarySubData();
            }

            if (submit) {
                this.$emit('submit', this.createdCashFlowLibrary);
            } else {
                this.$emit('submit', null);
            }

            this.createdCashFlowLibrary = clone({...emptyCashFlowLibrary, id: ObjectID.generate()});
        }

        /**
         * Generates bson ids for library's parameters & durations data
         */
        setIdsForNewLibrarySubData() {
            this.createdCashFlowLibrary.splitTreatments = this.createdCashFlowLibrary.splitTreatments
                .map((splitTreatment: SplitTreatment) => {
                    return clone({
                        ...splitTreatment,
                        id: ObjectID.generate(),
                        splitTreatmentLimits: splitTreatment.splitTreatmentLimits
                            .map((splitTreatmentLimit: SplitTreatmentLimit) => {
                                splitTreatmentLimit.id = ObjectID.generate();
                                return splitTreatmentLimit;
                            })
                    });
                });
        }
    }
</script>