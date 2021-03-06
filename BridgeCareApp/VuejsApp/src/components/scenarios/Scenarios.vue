<template>
    <v-layout column>
        <v-flex xs12>
            <v-card>
                <v-flex xs10>
                    <v-layout>
                        <div>
                            <v-data-table :headers="rollupGridHeader"
                                          :items="adminRollup"
                                          :items-per-page="5"
                                          hide-actions
                                          class="elevation-1">
                                <template slot="items" slot-scope="props">
                                    <td>{{ props.item.networkName }}</td>
                                    <td>{{ props.item.createdDate }}</td>
                                    <td>{{ props.item.lastModifiedDate }}</td>
                                    <td>{{ props.item.rollupStatus }}</td>
                                    <td>
                                        <v-layout row wrap>
                                            <v-flex>
                                                <v-btn icon class="green--text darken-2" @click="onShowRunRollupAlert(props.item)">
                                                    <v-icon>fas fa-play</v-icon>
                                                </v-btn>
                                            </v-flex>
                                        </v-layout>
                                    </td>
                                </template>
                            </v-data-table>
                        </div>
                        <div class="pad-button">
                            <v-btn round color="green darken-2 white--text" @click="onLoadNetworks()">Load networks</v-btn>
                        </div>
                    </v-layout>
                </v-flex>
                <v-divider></v-divider>
                <v-card-title>
                    <v-flex xs4>
                        <v-chip color="ara-blue-bg" text-color="white">
                            New Scenarios
                            <v-icon right>star</v-icon>
                        </v-chip>
                    </v-flex>
                    <v-spacer></v-spacer>
                    <v-flex xs6>
                        <v-text-field v-model="searchMine" append-icon="fas fa-search" lablel="Search" single-line hide-details>
                        </v-text-field>
                    </v-flex>
                    <v-flex xs-2>
                        <v-btn round class="ara-blue-bg white--text" @click="onUpdateScenarioList()">
                            Load legacy scenarios
                        </v-btn>
                    </v-flex>
                </v-card-title>
                <v-data-table :headers="scenarioGridHeaders" :items="userScenarios" :search="searchMine">
                    <template slot="items" slot-scope="props">
                        <td>
                            <v-edit-dialog :return-value.sync="props.item.simulationName"
                                           large lazy persistent
                                           @save="onEditScenarioName(props.item.simulationName, props.item.id, props.item.simulationId)">
                                {{props.item.simulationName}}
                                <template slot="input">
                                    <v-text-field v-model="props.item.simulationName"
                                                  label="Edit"
                                                  single-line></v-text-field>
                                </template>
                            </v-edit-dialog>
                        </td>
                        <td>{{formatDate(props.item.createdDate)}}</td>
                        <td>{{formatDate(props.item.lastModifiedDate)}}</td>
                        <td>{{props.item.status}}</td>
                        <td>
                            <v-layout row wrap>
                                <v-flex>
                                    <v-btn icon class="ara-blue" @click="onShowRunSimulationAlert(props.item)">
                                        <v-icon>fas fa-play</v-icon>
                                    </v-btn>
                                </v-flex>
                                <v-flex>
                                    <v-btn icon class="ara-blue" @click="onShowReportsDownloaderDialog(props.item)">
                                        <v-icon>fas fa-chart-line</v-icon>
                                    </v-btn>
                                </v-flex>
                                <v-flex>
                                    <v-btn icon class="edit-icon"
                                           @click="onEditScenario(props.item.simulationId, props.item.simulationName)">
                                        <v-icon>fas fa-edit</v-icon>
                                    </v-btn>
                                </v-flex>
                                <v-flex>
                                    <v-btn icon class="ara-orange"
                                           @click="onDeleteScenario(props.item.simulationId, props.item.id)">
                                        <v-icon>fas fa-trash</v-icon>
                                    </v-btn>
                                </v-flex>
                            </v-layout>
                        </td>
                    </template>
                    <v-alert class="ara-orange-bg" slot="no-results" :value="true" icon="fas fa-exclamation">
                        Your search for "{{searchMine}}" found no results.
                    </v-alert>
                </v-data-table>
                <v-card-actions>
                    <v-btn color="ara-blue-bg white--text" @click="onCreateScenario">Create new</v-btn>
                </v-card-actions>
            </v-card>
        </v-flex>

        <v-flex xs12>
            <v-card>
                <v-card-title>
                    Shared with Me
                    <v-spacer></v-spacer>
                    <v-text-field v-model="searchShared" append-icon="fas fa-search" lablel="Search" single-line
                                  hide-details>
                    </v-text-field>
                </v-card-title>
                <v-data-table :headers="scenarioGridHeaders" :items="sharedScenarios" :search="searchShared">
                    <template slot="items" slot-scope="props">
                        <td>{{props.item.simulationName}}</td>
                        <td>{{formatDate(props.item.createdDate)}}</td>
                        <td>{{formatDate(props.item.lastModifiedDate)}}</td>
                        <td>{{getStatus(props.item.status)}}</td>
                        <td>
                            <v-layout row wrap>
                                <v-flex>
                                    <v-btn flat icon class="ara-blue" @click="onShowRunSimulationAlert(props.item)">
                                        <v-icon>fas fa-play</v-icon>
                                    </v-btn>
                                </v-flex>
                                <v-flex>
                                    <v-btn flat icon class="ara-blue" @click="onShowReportsDownloaderDialog(props.item)">
                                        <v-icon>fas fa-chart-line</v-icon>
                                    </v-btn>
                                </v-flex>
                                <v-flex>
                                    <v-btn flat icon class="edit-icon"
                                           @click="onEditSharedScenario(props.item.scenarioId, props.item.simulationName)">
                                        <v-icon>fas fa-edit</v-icon>
                                    </v-btn>
                                </v-flex>
                            </v-layout>
                        </td>
                    </template>
                    <v-alert class="ara-orange-bg" slot="no-results" :value="true" icon="fas fa-exclamation">
                        Your search for "{{searchShared}}" found no results.
                    </v-alert>
                </v-data-table>
            </v-card>
        </v-flex>

        <Alert :dialogData="alertData" @submit="onSubmitAlertResult" />

        <Alert :dialogData="alertBeforeDelete" @submit="onSubmitResponse" />
        <Alert :dialogData="alertBeforeRunRollup" @submit="onSubmitRollupDecision" />

        <CreateScenarioDialog :showDialog="showCreateScenarioDialog" @submit="onSubmitNewScenario" />

        <ReportsDownloaderDialog :dialogData="reportsDownloaderDialogData" />
    </v-layout>
</template>

<script lang="ts">
    import Vue from 'vue';
    import {Component, Watch} from 'vue-property-decorator';
    import {Action, State} from 'vuex-class';
    import moment from 'moment';
    import {emptyScenario, Scenario} from '@/shared/models/iAM/scenario';
    import {hasValue} from '@/shared/utils/has-value-util';
    import {AlertData, emptyAlertData} from '@/shared/models/modals/alert-data';
    import Alert from '@/shared/modals/Alert.vue';
    import ReportsDownloaderDialog from '@/components/scenarios/scenarios-dialogs/ReportsDownloaderDialog.vue';
    import {
        emptyReportsDownloadDialogData,
        ReportsDownloaderDialogData
    } from '@/shared/models/modals/reports-downloader-dialog-data';
    import {ScenarioCreationData} from '@/shared/models/modals/scenario-creation-data';
    import CreateScenarioDialog from '@/components/scenarios/scenarios-dialogs/CreateScenarioDialog.vue';
    import {Network} from '@/shared/models/iAM/network';
    import { clone } from 'ramda';
    import { Simulation } from '../../shared/models/iAM/simulation';
    import { Rollup, emptyRollup } from '../../shared/models/iAM/rollup';

    @Component({
        components: {Alert, ReportsDownloaderDialog, CreateScenarioDialog}
    })
    export default class Scenarios extends Vue {
        @State(state => state.scenario.scenarios) scenarios: Scenario[];
        @State(state => state.authentication.userId) userId: string;
        @State(state => state.breadcrumb.navigation) navigation: any[];
        @State(state => state.network.networks) networks: Network[];
        @State(state => state.authentication.authenticated) authenticated: boolean;
        @State(state => state.rollup.rollups) rollups: Rollup[];
        
        @Action('getMongoScenarios') getMongoScenariosAction: any;
        @Action('getLegacyScenarios') getLegacyScenariosAction: any;
        @Action('runSimulation') runSimulationAction: any;
        @Action('createScenario') createScenarioAction: any;
        @Action('deleteScenario') deleteScenarioAction: any;
        @Action('updateScenario') updateScenarioAction: any;
        @Action('getSummaryReportMissingAttributes') getSummaryReportMissingAttributesAction: any;
        @Action('getMongoRollups') getMongoRollupsAction: any;
        @Action('rollupNetwork') rollupNetworkAction: any;
        @Action('getLegacyNetworks') getLegacyNetworksAction: any;

        alertData: AlertData = clone(emptyAlertData);
        alertBeforeDelete: AlertData = clone(emptyAlertData);
        alertBeforeRunRollup: AlertData = clone(emptyAlertData);
        reportsDownloaderDialogData: ReportsDownloaderDialogData = clone(emptyReportsDownloadDialogData);
        showCreateScenarioDialog: boolean = false;
        scenarioGridHeaders: object[] = [
            {text: 'Scenario Name', align: 'left', sortable: false, value: 'simulationName'},
            {text: 'Date Created', sortable: false, value: 'createdDate'},
            {text: 'Date Last Modified', sortable: false, value: 'lastModifiedDate' },
            {text: 'Status', sortable: false, value: 'status' },
            {text: '', sortable: false, value: 'actions'}
        ];
        rollupGridHeader: object[] = [
            {text: 'Network name', align: 'left', sortable: false, value: 'rollupName'},
            {text: 'Date Created', sortable: false, value: 'createdDate'},
            {text: 'Date Last Modified', sortable: false, value: 'lastModifiedDate' },
            {text: 'Status', sortable: false, value: 'rollupStatus' },
            {text: '', sortable: false, value: 'actions'}
        ];
        userScenarios: Scenario[] = [];
        adminRollup: any[] = [];
        sharedScenarios: Scenario[] = [];
        searchMine = '';
        searchShared = '';
        networkId: number = 0;
        networkName: string = '';
        simulationId: number = 0;
        simulationName: string = '';
        scenarioId: string = '';
        currentScenario: Scenario = clone(emptyScenario);
        currentRollup: Rollup = clone(emptyRollup);

        @Watch('scenarios')
        onScenariosChanged() {
            if (hasValue(this.scenarios)) {
                // filter scenarios that are the user's
                this.userScenarios = this.scenarios.filter((simulation: Scenario) => !simulation.shared);
                // filter scenarios that are shared with the user
                this.sharedScenarios = this.scenarios.filter((simulation: Scenario) => simulation.shared);
            }
            else {
                this.userScenarios = [];
            }

        }

        @Watch('rollups')
        onRollupsChanged() {
            if (hasValue(this.rollups)) {
                this.adminRollup = this.rollups;
            }
            else {
                this.adminRollup = [];
            }

        }

        @Watch('authenticated')
        onAuthenticated() {
            if (this.authenticated) {
                this.getMongoScenariosAction({ userId: this.userId });
                this.getMongoRollupsAction({ });
            }
        }

        /**
         * Component has been mounted
         */
        mounted() {
            if (this.authenticated) {
                this.getMongoScenariosAction({ userId: this.userId });
                this.getMongoRollupsAction({ });
            }
        }

        onUpdateScenarioList() {
            this.getLegacyScenariosAction();
        }

        onLoadNetworks() {
            this.getLegacyNetworksAction({networks: this.adminRollup });
        }

        getStatus(isCompleted: boolean) {
            return isCompleted ? 'Completed' : 'Running';
        }

        /**
         * Formats a date as month/day/year
         * @param unformattedDate Unformatted date
         */
        formatDate(unformattedDate: Date) {
            return moment(unformattedDate).format('M/D/YYYY');
        }

        /**
         * Navigates user to EditScenario page passing in the simulation id of their scenario
         * @param id Scenario simulation id
         */
        onEditScenario(id: number, simulationName: string) {
            this.$router.push({
                path: '/EditScenario/', query: {selectedScenarioId: id.toString(), simulationName: simulationName}
            });
        }

        onDeleteScenario(simulationId: number, id: string) {

            this.alertBeforeDelete = {
                showDialog: true,
                heading: 'Warning',
                choice: true,
                message: 'Are you sure you want to delete?'
            };

            this.simulationId = simulationId;
            this.scenarioId = id;
        }

        onSubmitResponse(response: boolean) {
            this.alertBeforeDelete = clone(emptyAlertData);
            
            if (response) {
                this.deleteScenario();
            }
        }

        deleteScenario() {
            this.deleteScenarioAction({
                simulationId: this.simulationId,
                scenarioId: this.scenarioId
            });
        }
        
        /**
         * Navigates user to EditScenario page passing in the simulation id of a shared scenario
         * @param id Scenario simulation id
         */
        onEditSharedScenario(id: number, simulationName: string) {
            this.$router.push({
                path: '/EditScenario/', query: {selectedScenarioId: id.toString(), simulationName: simulationName}
            });
        }

        /**
         * Shows the Alert
         */
        onShowRunSimulationAlert(scenario: Scenario) {
            this.currentScenario = scenario;
            
            this.alertData = {
                showDialog: true,
                heading: 'Warning',
                choice: true,
                message: 'The simulation can take around five minutes to finish. ' +
                    'Are you sure that you want to continue?'
            };
        }

        /**
         * Shows the Alert
         */
        onShowRunRollupAlert(rollup: Rollup) {
            this.currentRollup = rollup;
            this.alertBeforeRunRollup = {
                showDialog: true,
                heading: 'Warning',
                choice: true,
                message: 'The rollup can take around five minutes to finish. ' +
                    'Are you sure that you want to continue?'
            };
        }

        onSubmitRollupDecision(response: boolean) {
            this.alertBeforeRunRollup = clone(emptyAlertData);
            
            if (response) {
                this.rollupNetwork();
            }
        }

        /**
         * Takes in a boolean parameter from the AppPopupModal to determine if a scenario's simulation should be executed
         * @param runScenarioSimulation Alert result
         */
        onSubmitAlertResult(runScenarioSimulation: boolean) {
            this.alertData = clone(emptyAlertData);
            
            if (runScenarioSimulation) {
                this.runScenarioSimulation();
            }
        }

        /**
         * Dispatches an action with the currentScenario object's data in order to run a simulation on the server
         */
        runScenarioSimulation() {
            this.runSimulationAction({
                selectedScenario: this.currentScenario,
                userId: this.userId
            });
        }

        rollupNetwork() {
            this.rollupNetworkAction({
                selectedNetwork: this.currentRollup
            });
        }

        /**
         * Shows the ReportsDownloaderDialog passing in the specified scenario's data
         * @param scenario Scenario object to use for setting the ReportsDownloaderDialogData object
         */
        onShowReportsDownloaderDialog(scenario: Scenario) {
            this.getSummaryReportMissingAttributesAction({
                selectedScenarioId: scenario.simulationId, selectedNetworkId: this.networks[0].networkId}
            ).then(() => {
                setTimeout(() => {
                    this.reportsDownloaderDialogData = {
                        showModal: true,
                        scenario: scenario
                    };
                });
            });
        }

        onCreateScenario() {
            this.showCreateScenarioDialog = true;
        }

        onEditScenarioName(scenarioName: string, id: string, simulationId: any) {
            var scenarioData : Simulation = {
                simulationId: simulationId,
                simulationName: scenarioName,
                networkId: this.networks[0].networkId,
                networkName: this.networks[0].networkName
            };
            this.updateScenarioAction({
                updateScenarioData: scenarioData,
                scenarioId: id
            });
        }

        onSubmitNewScenario(createScenarioData: ScenarioCreationData) {
            this.showCreateScenarioDialog = false;

            if (hasValue(createScenarioData)) {
                this.createScenarioAction({
                    createScenarioData: {...createScenarioData, networkId: this.networks[0].networkId},
                    userId: this.userId
                });
            }

        }
    }
</script>

<style>
    .pad-button{
        padding-top:33px;
    }
</style>