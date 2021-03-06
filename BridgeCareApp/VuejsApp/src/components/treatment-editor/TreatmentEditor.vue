<template>
    <v-layout column>
        <v-flex xs12>
            <v-layout justify-center>
                <v-flex xs3>
                    <v-btn v-show="selectedScenarioId === '0'" class="ara-blue-bg white--text" @click="onNewLibrary">
                        New Library
                    </v-btn>
                    <v-select v-if="!hasSelectedTreatmentLibrary || selectedScenarioId !== '0'"
                              :items="treatmentLibrariesSelectListItems" label="Select a Treatment Library"
                              outline v-model="treatmentLibrarySelectItemValue" class="treatment-library-select">
                    </v-select>
                    <v-text-field v-if="hasSelectedTreatmentLibrary && selectedScenarioId === '0'"
                                  label="Treatment Name" v-model="selectedTreatmentLibrary.name">
                        <template slot="append">
                            <v-btn class="ara-orange" icon @click="onClearSelectedTreatmentLibrary">
                                <v-icon>fas fa-times</v-icon>
                            </v-btn>
                        </template>
                    </v-text-field>
                </v-flex>
            </v-layout>
        </v-flex>
        <v-divider v-show="hasSelectedTreatmentLibrary"></v-divider>
        <v-flex xs12 v-show="hasSelectedTreatmentLibrary">
            <div class="treatments-div">
                <v-layout justify-center row>
                    <v-flex xs3>
                        <v-btn class="ara-blue-bg white--text" @click="onAddTreatment">
                            Add Treatment
                        </v-btn>
                        <v-select :items="treatmentsSelectListItems" label="Select a Treatment" outline
                                  clearable v-model="treatmentSelectItemValue">
                        </v-select>
                    </v-flex>
                    <v-flex xs9>
                        <div v-show="selectedTreatment.id !== '0'">
                            <v-tabs v-model="activeTab">
                                <v-tab v-for="(treatmentTab, index) in treatmentTabs" :key="index" ripple
                                       @click="setAsActiveTab(index)">
                                    {{treatmentTab}}
                                </v-tab>
                                <v-tabs-items v-model="activeTab">
                                    <v-tab-item>
                                        <v-card>
                                            <v-card-text class="card-tab-content">
                                                <FeasibilityTab :feasibilityTabData="tabData" @submit="updateSelectedTreatmentLibrary" />
                                            </v-card-text>
                                        </v-card>
                                    </v-tab-item>
                                    <v-tab-item>
                                        <v-card>
                                            <v-card-text class="card-tab-content">
                                                <CostsTab :costsTabData="tabData" @submit="updateSelectedTreatmentLibrary" />
                                            </v-card-text>
                                        </v-card>
                                    </v-tab-item>
                                    <v-tab-item>
                                        <v-card>
                                            <v-card-text class="card-tab-content">
                                                <ConsequencesTab :consequencesTabData="tabData" @submit="updateSelectedTreatmentLibrary" />
                                            </v-card-text>
                                        </v-card>
                                    </v-tab-item>
                                    <v-tab-item>
                                        <v-card>
                                            <v-card-text class="card-tab-content">
                                                <BudgetsTab :budgetsTabData="tabData" @submit="updateSelectedTreatmentLibrary" />
                                            </v-card-text>
                                        </v-card>
                                    </v-tab-item>
                                </v-tabs-items>
                            </v-tabs>
                        </div>
                    </v-flex>
                </v-layout>
            </div>
        </v-flex>
        <v-divider v-show="hasSelectedTreatmentLibrary"></v-divider>
        <v-flex xs12 v-show="hasSelectedTreatmentLibrary && (scenarioTreatmentLibrary === null || selectedTreatmentLibrary.id !== scenarioTreatmentLibrary.id)">
            <v-layout justify-center>
                <v-flex xs6>
                    <v-textarea rows="4" no-resize outline label="Description"
                                v-model="selectedTreatmentLibrary.description">
                    </v-textarea>
                </v-flex>
            </v-layout>
        </v-flex>
        <v-flex xs12>
            <v-layout v-show="hasSelectedTreatmentLibrary" justify-end row>
                <v-btn v-show="selectedScenarioId !== '0'" class="ara-blue-bg white--text" @click="onApplyToScenario"
                       :disabled="!hasSelectedTreatmentLibrary">
                    Save
                </v-btn>
                <v-btn v-show="selectedScenarioId === '0'" class="ara-blue-bg white--text" @click="onUpdateLibrary"
                       :disabled="!hasSelectedTreatmentLibrary">
                    Update Library
                </v-btn>
                <v-btn class="ara-blue-bg white--text" @click="onCreateAsNewLibrary" :disabled="!hasSelectedTreatmentLibrary">
                    Create as New Library
                </v-btn>
                <v-btn v-show="selectedScenarioId !== '0'" class="ara-orange-bg white--text" @click="onDiscardChanges"
                       :disabled="!hasSelectedTreatmentLibrary">
                    Discard Changes
                </v-btn>
            </v-layout>
        </v-flex>

        <CreateTreatmentLibraryDialog :dialogData="createTreatmentLibraryDialogData"
                                      @submit="onCreateTreatmentLibrary" />

        <CreateTreatmentDialog :showDialog="showCreateTreatmentDialog" @submit="onCreateTreatment" />
    </v-layout>
</template>

<script lang="ts">
    import Vue from 'vue';
    import Component from 'vue-class-component';
    import {Watch} from 'vue-property-decorator';
    import {State, Action} from 'vuex-class';
    import CreateTreatmentLibraryDialog from '@/components/treatment-editor/treatment-editor-dialogs/CreateTreatmentLibraryDialog.vue';
    import {SelectItem} from '@/shared/models/vue/select-item';
    import {
        CreateTreatmentLibraryDialogData, emptyCreateTreatmentLibraryDialogData
    } from '@/shared/models/modals/create-treatment-library-dialog-data';
    import {
        Consequence,
        Cost,
        emptyFeasibility,
        emptyTreatment, emptyTreatmentLibrary, Treatment, TreatmentLibrary
    } from '@/shared/models/iAM/treatment';
    import {hasValue} from '@/shared/utils/has-value-util';
    import CreateTreatmentDialog from '@/components/treatment-editor/treatment-editor-dialogs/CreateTreatmentDialog.vue';
    import {isNil, append, any, propEq, clone, isEmpty} from 'ramda';
    import FeasibilityTab from '@/components/treatment-editor/treatment-editor-tabs/FeasibilityTab.vue';
    import CostsTab from '@/components/treatment-editor/treatment-editor-tabs/CostsTab.vue';
    import {TabData, emptyTabData} from '@/shared/models/child-components/tab-data';
    import ConsequencesTab from '@/components/treatment-editor/treatment-editor-tabs/ConsequencesTab.vue';
    import BudgetsTab from '@/components/treatment-editor/treatment-editor-tabs/BudgetsTab.vue';
    import {InvestmentLibrary} from '@/shared/models/iAM/investment';
    import {sortByProperty, sorter} from '@/shared/utils/sorter-utils';
    import {getPropertyValues} from '@/shared/utils/getter-utils';
    const ObjectID = require('bson-objectid');

    @Component({
        components: {
            BudgetsTab,
            ConsequencesTab, CostsTab, FeasibilityTab, CreateTreatmentDialog, CreateTreatmentLibraryDialog}
    })
    export default class TreatmentEditor extends Vue {
        @State(state => state.treatmentEditor.treatmentLibraries) stateTreatmentLibraries: TreatmentLibrary[];
        @State(state => state.treatmentEditor.selectedTreatmentLibrary) stateSelectedTreatmentLibrary: TreatmentLibrary;
        @State(state => state.treatmentEditor.scenarioTreatmentLibrary) stateScenarioTreatmentLibrary: TreatmentLibrary;
        @State(state => state.investmentEditor.scenarioInvestmentLibrary) scenarioInvestmentLibrary: InvestmentLibrary;

        @Action('getTreatmentLibraries') getTreatmentLibrariesAction: any;
        @Action('getScenarioTreatmentLibrary') getScenarioTreatmentLibraryAction: any;
        @Action('selectTreatmentLibrary') selectTreatmentLibraryAction: any;
        @Action('updateSelectedTreatmentLibrary') updateSelectedTreatmentLibraryAction: any;
        @Action('createTreatmentLibrary') createTreatmentLibraryAction: any;
        @Action('updateTreatmentLibrary') updateTreatmentLibraryAction: any;
        @Action('saveScenarioTreatmentLibrary') saveScenarioTreatmentLibraryAction: any;
        @Action('getScenarioInvestmentLibrary') getScenarioInvestmentLibraryAction: any;

        treatmentLibraries: TreatmentLibrary[] = [];
        selectedTreatmentLibrary: TreatmentLibrary = clone(emptyTreatmentLibrary);
        scenarioTreatmentLibrary: TreatmentLibrary = clone(emptyTreatmentLibrary);
        selectedScenarioId: string = '0';
        hasSelectedTreatmentLibrary: boolean = false;
        treatmentLibrariesSelectListItems: SelectItem[] = [];
        treatmentLibrarySelectItemValue: string = '';
        treatmentsSelectListItems: SelectItem[] = [];
        treatmentSelectItemValue: string = '';
        selectedTreatment: Treatment = clone(emptyTreatment);
        activeTab: number = 0;
        treatmentTabs: string[] = ['feasibility', 'costs', 'consequences'];
        createTreatmentLibraryDialogData: CreateTreatmentLibraryDialogData = clone(emptyCreateTreatmentLibraryDialogData);
        showCreateTreatmentDialog: boolean = false;
        tabData: TabData = clone(emptyTabData);

        /**
         * Sets component ui properties that triggers cascading ui updates
         */
        beforeRouteEnter(to: any, from: any, next: any) {
            next((vm: any) => {
                if (to.path === '/TreatmentEditor/Scenario/') {
                    vm.selectedScenarioId = to.query.selectedScenarioId;

                    if (vm.selectedScenarioId === '0') {
                        // set 'no selected scenario' error message, then redirect user to Scenarios UI
                        vm.setErrorMessageAction({message: 'Found no selected scenario for edit'});
                        vm.$router.push('/Scenarios/');
                    }

                    vm.treatmentTabs = [...vm.treatmentTabs, 'budgets'];
                }

                vm.onClearSelectedTreatmentLibrary();

                setTimeout(() => {
                    vm.getTreatmentLibrariesAction()
                        .then(() => {
                            setTimeout(() => {
                                if (vm.selectedScenarioId !== '0') {
                                    vm.getScenarioTreatmentLibraryAction({selectedScenarioId: vm.selectedScenarioId})
                                        .then(() => {
                                            if (vm.scenarioInvestmentLibrary.id !== vm.selectedScenarioId) {
                                                vm.getScenarioInvestmentLibraryAction({
                                                    selectedScenarioId: vm.selectedScenarioId
                                                });
                                            }
                                        });
                                }
                            });
                        });
                }, 0);
            });
        }

        /**
         * Resets component ui properties that triggers cascading ui updates
         */
        beforeRouteUpdate(to: any, from: any, next: any) {
            if (to.path === '/TreatmentEditor/Library/') {
                this.selectedScenarioId = '0';
                this.onClearSelectedTreatmentLibrary();
                if (this.treatmentTabs.length === 4) {
                    this.treatmentTabs.splice(3, 1);
                }
                next();
            }
        }

        /**
         * Sets treatmentLibraries with the treatmentLibraries state data
         */
        @Watch('stateTreatmentLibraries')
        onStateTreatmentLibrariesChanged() {
            this.treatmentLibraries = clone(this.stateTreatmentLibraries);
        }

        /**
         * Sets selectedTreatmentLibrary with selectedTreatmentLibrary state data
         */
        @Watch('stateSelectedTreatmentLibrary')
        onStateSelectedTreatmentLibraryChanged() {
            this.selectedTreatmentLibrary = clone(this.stateSelectedTreatmentLibrary);
        }

        /**
         * Sets scenarioTreatmentLibrary with scenarioTreatmentLibrary state data
         */
        @Watch('stateScenarioTreatmentLibrary')
        onStateScenarioTreatmentLibraryChanged() {
            this.scenarioTreatmentLibrary = clone(this.stateScenarioTreatmentLibrary);
        }

        /**
         * Sets the treatmentLibrariesSelectListItems using treatmentLibraries, then calls setAllTreatments to push
         * all treatment libraries into one list
         */
        @Watch('treatmentLibraries')
        onTreatmentLibrariesChanged() {
            this.treatmentLibrariesSelectListItems = this.treatmentLibraries
                .map((treatmentLibrary: TreatmentLibrary) => ({
                    text: treatmentLibrary.name,
                    value: treatmentLibrary.id.toString()
                }));
        }

        /**
         * Sets the selected treatment library based on treatmentLibrarySelectItemValue
         */
        @Watch('treatmentLibrarySelectItemValue')
        onTreatmentLibrarySelectItemValueChanged() {
            this.selectTreatmentLibraryAction({treatmentLibraryId: this.treatmentLibrarySelectItemValue});
        }

        /**
         * Sets/resets the component UI properties reliant on a selected treatment library
         */
        @Watch('selectedTreatmentLibrary')
        onSelectedTreatmentLibraryChanged() {
            if (this.selectedTreatmentLibrary.id !== '0') {
                this.hasSelectedTreatmentLibrary = true;

                if (!hasValue(this.selectedTreatmentLibrary.treatments) ||
                    !any(propEq('id', this.selectedTreatment.id), this.selectedTreatmentLibrary.treatments)) {
                    this.treatmentSelectItemValue = '';
                }

                this.treatmentsSelectListItems = hasValue(this.selectedTreatmentLibrary.treatments)
                    ? this.selectedTreatmentLibrary.treatments.map((treatment: Treatment) => ({
                            text: treatment.name,
                            value: treatment.id.toString()
                        }))
                    : [];

                if (this.selectedTreatmentLibrary.id !== this.scenarioTreatmentLibrary.id) {
                    this.setTreatmentBudgets();
                }
            } else {
                this.hasSelectedTreatmentLibrary = false;
                this.treatmentSelectItemValue = '';
                this.treatmentsSelectListItems = [];
            }
        }

        setTreatmentBudgets() {
            let budgets: string[] = [];
            if (!isEmpty(this.scenarioInvestmentLibrary.budgetOrder)) {
                budgets = clone(this.scenarioInvestmentLibrary.budgetOrder);
            } else if (!isEmpty(this.scenarioInvestmentLibrary.budgetYears)) {
                budgets = sorter(getPropertyValues('budgetName', this.scenarioInvestmentLibrary.budgetYears)) as string[];
            }

            this.selectedTreatmentLibrary.treatments.forEach((treatment: Treatment) => treatment.budgets = clone(budgets));
        }

        /**
         * Sets the selected treatment
         */
        @Watch('treatmentSelectItemValue')
        onTreatmentSelectItemValueChanged() {
            this.setSelectedTreatment(this.treatmentSelectItemValue);
        }

        /**
         * Sets selectedTreatment
         * @param id Treatment id to use in setting the selected treatment
         */
        setSelectedTreatment(id: any) {
            if (any(propEq('id', id), this.selectedTreatmentLibrary.treatments as Treatment[])) {
                this.selectedTreatment = clone(
                    this.selectedTreatmentLibrary.treatments.find((t: Treatment) => t.id === id) as Treatment
                );
            } else {
                this.selectedTreatment = clone(emptyTreatment);
            }

            this.setTabData();
        }

        /**
         * Sets data for each of the child component tabs
         */
        setTabData() {
            this.tabData = {
                tabTreatmentLibraries: hasValue(this.treatmentLibraries) ? clone(this.treatmentLibraries) : [],
                tabSelectedTreatmentLibrary: clone(this.selectedTreatmentLibrary),
                tabSelectedTreatment: clone(this.selectedTreatment),
                tabScenarioInvestmentLibrary: clone(this.scenarioInvestmentLibrary)
            };
        }

        /**
         * Clears the selected treatment library and resets the active tab to first tab
         */
        onClearSelectedTreatmentLibrary() {
            this.treatmentLibrarySelectItemValue = hasValue(this.treatmentLibrarySelectItemValue) ? '' : '0';
            this.activeTab = 0;
        }

        /**
         * Shows the CreateTreatmentLibraryDialog to allow a user to create a new treatment library
         */
        onNewLibrary() {
            this.createTreatmentLibraryDialogData = {
                ...clone(emptyCreateTreatmentLibraryDialogData),
                showDialog: true
            };
        }

        /**
         * Shows the CreateTreatmentDialog to allow a user to create a new treatment
         */
        onAddTreatment() {
            this.showCreateTreatmentDialog = true;
        }

        /**
         * Sets the active tab
         */
        setAsActiveTab(treatmentTab: number) {
            this.activeTab = treatmentTab;
        }

        /**
         * Dispatches an action to update the selected treatment library in state, then resets the selected treatment
         * @param updatedSelectedTreatmentLibrary The selected treatment library data to use for updating
         */
        updateSelectedTreatmentLibrary(updatedSelectedTreatmentLibrary: TreatmentLibrary) {
            this.updateSelectedTreatmentLibraryAction({
                updatedSelectedTreatmentLibrary: updatedSelectedTreatmentLibrary
            }).then(() => {
                setTimeout(() => {
                    this.setSelectedTreatment(this.selectedTreatment.id);
                });
            });
        }

        /**
         * Shows the CreateTreatmentLibraryDialog passing in the selected treatment library's description & treatments data
         */
        onCreateAsNewLibrary() {
            this.createTreatmentLibraryDialogData = {
                showDialog: true,
                selectedTreatmentLibraryDescription: this.selectedTreatmentLibrary.description,
                selectedTreatmentLibraryTreatments: this.selectedTreatmentLibrary.treatments
            };
        }

        /**
         * Dispatches an action to create a new treatment library on the server
         */
        onCreateTreatmentLibrary(createdTreatmentLibrary: TreatmentLibrary) {
            this.createTreatmentLibraryDialogData =  clone(emptyCreateTreatmentLibraryDialogData);

            if (!isNil(createdTreatmentLibrary)) {
                createdTreatmentLibrary.id = ObjectID.generate();
                createdTreatmentLibrary = this.setIdsForNewTreatmentLibraryRelatedData(createdTreatmentLibrary);

                this.createTreatmentLibraryAction({createdTreatmentLibrary: createdTreatmentLibrary})
                    .then(() => {
                        setTimeout(() => {
                            this.onClearSelectedTreatmentLibrary();
                            setTimeout(() => {
                                this.treatmentLibrarySelectItemValue = createdTreatmentLibrary.id.toString();
                            });
                        });
                    });
            }
        }

        /**
         * Sets the ids for the createdPerformanceLibrary object's equations
         */
        setIdsForNewTreatmentLibraryRelatedData(createdTreatmentLibrary: TreatmentLibrary) {
            if (hasValue(createdTreatmentLibrary.treatments)) {
                createdTreatmentLibrary.treatments = sortByProperty('id', createdTreatmentLibrary.treatments)
                    .map((treatment: Treatment) => {
                        treatment.id = ObjectID.generate();
                        if (hasValue(treatment.feasibility)) {
                            treatment.feasibility.id = ObjectID.generate();
                        } else {
                            treatment.feasibility = {
                                ...emptyFeasibility,
                                id: ObjectID.generate()
                            };
                        }
                        treatment.costs = treatment.costs.map((cost: Cost) => {
                            cost.id = ObjectID.generate();
                            return cost;
                        });
                        treatment.consequences = treatment.consequences.map((consequence: Consequence) => {
                            consequence.id = ObjectID.generate();
                            return consequence;
                        });
                        return treatment;
                    });
            }

            return createdTreatmentLibrary;
        }

        /**
         * Dispatches an action to add a created treatment to the selected treatment library's treatments list in state
         */
        onCreateTreatment(createdTreatment: Treatment) {
            this.showCreateTreatmentDialog = false;

            if (!isNil(createdTreatment)) {
                createdTreatment.id = ObjectID.generate();
                createdTreatment.feasibility.id = ObjectID.generate();
                this.selectedTreatmentLibrary.treatments = append(createdTreatment, this.selectedTreatmentLibrary.treatments);

                this.updateSelectedTreatmentLibraryAction({
                    updatedSelectedTreatmentLibrary: this.selectedTreatmentLibrary
                })
                .then(() => setTimeout(() => {
                    this.treatmentSelectItemValue = createdTreatment.id.toString();
                }));
            }
        }

        /**
         * Dispatches an action to update the selected treatment library on the server
         */
        onUpdateLibrary() {
            this.updateTreatmentLibraryAction({updatedTreatmentLibrary: this.selectedTreatmentLibrary});
        }

        /**
         * Dispatches an action to update the scenario's treatment library data with the currently selected treatment library
         */
        onApplyToScenario() {
            this.saveScenarioTreatmentLibraryAction({saveScenarioTreatmentLibraryData: {
                    ...clone(this.selectedTreatmentLibrary),
                    id: this.selectedScenarioId,
                    name: this.scenarioTreatmentLibrary.name
                }
            }).then(() => setTimeout(() => {
                if (hasValue(this.treatmentLibrarySelectItemValue) && this.treatmentLibrarySelectItemValue !== '0') {
                    this.onDiscardChanges();
                }
            }));
        }

        /**
         * Clears the selected treatment library and dispatches an action to update the selected treatment library in
         * state with the scenario treatment library (if present), otherwise an action is dispatched to get the scenario
         * treatment library from the server to update the selected treatment library in state
         */
        onDiscardChanges() {
            this.onClearSelectedTreatmentLibrary();

            if (this.scenarioTreatmentLibrary.id !== '0') {
                setTimeout(() => {
                    this.updateSelectedTreatmentLibraryAction({
                        updatedSelectedTreatmentLibrary: this.scenarioTreatmentLibrary
                    });
                });
            } else {
                setTimeout(() => {
                    this.getScenarioTreatmentLibraryAction({selectedScenarioId: this.selectedScenarioId});
                });
            }
        }
    }
</script>

<style>
    .treatment-editor-container {
        height: 730px;
        overflow-x: hidden;
        overflow-y: auto;
    }

    .treatments-div {
        height: 355px;
    }

    .card-tab-content {
        height: 305px;
        overflow-x: hidden;
        overflow-y: auto;
    }

    .treatment-library-select {
        height: 60px;
    }
</style>