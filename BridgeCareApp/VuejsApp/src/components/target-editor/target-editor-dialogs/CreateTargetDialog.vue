<template>
    <v-dialog v-model="showDialog" persistent max-width="450px">
        <v-card>
            <v-card-title>
                <v-layout justify-center>
                    <h3>New Target</h3>
                </v-layout>
            </v-card-title>
            <v-card-text>
                <v-layout column>
                    <v-text-field label="Name" v-model="newTarget.name" outline></v-text-field>

                    <v-select label="Select Attribute" :items="numericAttributes"
                              v-model="newTarget.attribute" outline>
                    </v-select>

                    <v-text-field v-model="newTarget.year" label="Year" outline :mask="'####'"></v-text-field>

                    <v-text-field label="Target" v-model="newTarget.targetMean" outline>
                    </v-text-field>
                </v-layout>
            </v-card-text>
            <v-card-actions>
                <v-layout justify-space-between row>
                    <v-btn class="ara-blue-bg white--text" @click="onSubmit(true)" :disabled="disableSubmit()">
                        Save
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
    import {Component, Prop, Watch} from 'vue-property-decorator';
    import {State} from 'vuex-class';
    import {Target, emptyTarget} from '@/shared/models/iAM/target';
    import {Attribute} from '@/shared/models/iAM/attribute';
    import {getPropertyValues} from '@/shared/utils/getter-utils';
    import {hasValue} from '@/shared/utils/has-value-util';
    import {clone} from 'ramda';
    import moment from 'moment';
    const ObjectID = require('bson-objectid');
    @Component
    export default class CreateTargetDialog extends Vue {
        @Prop() showDialog: boolean;

        @State(state => state.attribute.numericAttributes) stateNumericAttributes: Attribute[];

        newTarget: Target = clone({...emptyTarget, id: ObjectID.generate(), year: moment().year()});
        numericAttributes: string[] = [];
        showDatePicker: boolean = false;
        year: string = moment().year().toString();

        /**
         * Component mounted event handler
         */
        mounted() {
            if (hasValue(this.stateNumericAttributes)) {
                this.numericAttributes = getPropertyValues('name', this.stateNumericAttributes);
            }
        }

        /**
         * Sets the numericAttributes list property with a copy of the stateNumericAttributes list property
         */
        @Watch('stateNumericAttributes')
        onStateNumericAttributesChanged() {
            if (hasValue(this.stateNumericAttributes)) {
                this.numericAttributes = getPropertyValues('name', this.stateNumericAttributes);
            }
        }

        /**
         * Whether or not to disable the 'Submit' button
         */
        disableSubmit() {
            return !hasValue(this.newTarget.name) || !hasValue(this.newTarget.attribute) ||
                   !hasValue(this.newTarget.year) || !hasValue(this.newTarget.targetMean);
        }

        /**
         * Emits the newTarget object or a null value to the parent component and resets the newTarget object
         * @param submit Whether or not to emit the newTarget object
         */
        onSubmit(submit: boolean) {
            if (submit) {
                this.$emit('submit',this.newTarget);
            } else {
                this.$emit('submit', null);
            }

            this.newTarget = clone({...emptyTarget, id: ObjectID.generate(), year: moment().year()});
        }
    }
</script>