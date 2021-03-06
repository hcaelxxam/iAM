﻿<template>
    <v-layout column>
        <v-flex xs12>
            <v-layout fixed justify-space-between>
                <div>
                    <v-tabs>
                        <v-tab v-for="navigationTab in navigationTabs"
                               :key="navigationTab.tabName"
                               :to="navigationTab.navigation">
                            {{navigationTab.tabName}}
                            <v-icon right>{{navigationTab.tabIcon}}</v-icon>
                        </v-tab>
                    </v-tabs>
                </div>

                <div>
                    <v-btn class="ara-blue-bg white--text" @click="onShowCommittedProjectsFileUploader">
                        Committed Projects<v-icon right class="white--text">fas fa-cloud-upload-alt</v-icon>
                    </v-btn>
                </div>
            </v-layout>
        </v-flex>

        <v-flex xs12>
            <v-container fluid grid-list-xs>
                <router-view></router-view>
            </v-container>
        </v-flex>

        <CommittedProjectsFileUploaderDialog :showDialog="showFileUploader" @submit="onUploadCommittedProjectFiles" />
    </v-layout>
</template>

<script lang="ts">
    import Vue from 'vue';
    import Component from 'vue-class-component';
    import {State, Action} from 'vuex-class';
    import {Scenario} from '@/shared/models/iAM/scenario';
    import CommittedProjectsFileUploaderDialog from '@/components/scenarios/scenarios-dialogs/CommittedProjectsFileUploaderDialog.vue';
    import {isNil, any} from 'ramda';
    import {AxiosResponse} from 'axios';
    import CommittedProjectsService from '@/services/committed-projects.service';
    import {Network} from '@/shared/models/iAM/network';
    import FileDownload from 'js-file-download';
    import {NavigationTab} from '@/shared/models/iAM/navigation-tab';
    import {hasValue} from '@/shared/utils/has-value-util';
    import {CommittedProjectsDialogResult} from '@/shared/models/modals/committed-projects-dialog-result';

    @Component({
        components: { CommittedProjectsFileUploaderDialog }
    })
    export default class EditScenario extends Vue {
        @State(state => state.breadcrumb.navigation) navigation: any[];        
        @State(state => state.network.networks) networks: Network[];

        @Action('setErrorMessage') setErrorMessageAction: any;
        @Action('setSuccessMessage') setSuccessMessageAction: any;
        @Action('setSelectedScenarioName') setSelectedScenarioNameAction: any;

        selectedScenarioId: number = 0;
        showFileUploader: boolean = false;
        networkId: number = 0;
        selectedScenario: Scenario = {
            id: 0,
            simulationId: this.selectedScenarioId,
            networkId: this.networkId,
            simulationName: '',
            networkName: ''
        };
        navigationTabs: NavigationTab[] = [];

        beforeRouteEnter(to: any, from: any, next: any) {
            next((vm: any) => {
                // set selectedScenarioId
                vm.selectedScenarioId = isNaN(to.query.selectedScenarioId) ? 0 : parseInt(to.query.selectedScenarioId);
                vm.simulationName = to.query.simulationName;

                // check that selectedScenarioId is set
                if (vm.selectedScenarioId === 0) {
                    // set 'no selected scenario' error message, then redirect user to Scenarios UI
                    vm.setErrorMessageAction({ message: 'Found no selected scenario for edit' });
                    vm.$router.push('/Scenarios/');
                } else {
                    vm.setSelectedScenarioNameAction({selectedScenarioName: to.query.simulationName});

                    vm.navigationTabs = [
                        {
                            tabName: 'Analysis',
                            tabIcon: 'fas fa-chart-bar',
                            navigation: {
                                path: '/EditAnalysis/',
                                query: {selectedScenarioId: to.query.selectedScenarioId, simulationName: to.query.simulationName}
                            }
                        },
                        {
                            tabName: 'Investment',
                            tabIcon: 'fas fa-dollar-sign',
                            navigation: {
                                path: '/InvestmentEditor/Scenario/',
                                query: {selectedScenarioId: to.query.selectedScenarioId, simulationName: to.query.simulationName}
                            }
                        },
                        {
                            tabName: 'Performance',
                            tabIcon: 'fas fa-chart-line',
                            navigation: {
                                path: '/PerformanceEditor/Scenario/',
                                query: {selectedScenarioId: to.query.selectedScenarioId, simulationName: to.query.simulationName}
                            }
                        },
                        {
                            tabName: 'Treatment',
                            tabIcon: 'fas fa-heartbeat',
                            navigation: {
                                path: '/TreatmentEditor/Scenario/',
                                query: {selectedScenarioId: to.query.selectedScenarioId, simulationName: to.query.simulationName}
                            }
                        },
                        {
                            tabName: 'Priority',
                            tabIcon: 'fas fa-copy',
                            navigation: {
                                path: '/PriorityEditor/Scenario/',
                                query: {selectedScenarioId: to.query.selectedScenarioId, simulationName: to.query.simulationName}
                            }
                        },
                        {
                            tabName: 'Target',
                            tabIcon: 'fas fa-bullseye',
                            navigation: {
                                path: '/TargetEditor/Scenario/',
                                query: {selectedScenarioId: to.query.selectedScenarioId, simulationName: to.query.simulationName}
                            }
                        },
                        {
                            tabName: 'Deficient',
                            tabIcon: 'fas fa-level-down-alt',
                            navigation: {
                                path: '/DeficientEditor/Scenario/',
                                query: {selectedScenarioId: to.query.selectedScenarioId, simulationName: to.query.simulationName}
                            }
                        },
                        {
                            tabName: 'Remaining Life Limit',
                            tabIcon: 'fas fa-business-time',
                            navigation: {
                                path: '/RemainingLifeLimitEditor/Scenario/',
                                query: {selectedScenarioId: to.query.selectedScenarioId, simulationName: to.query.simulationName}
                            }
                        },
                        {
                            tabName: 'Cash Flow',
                            tabIcon: 'fas fa-money-bill-wave',
                            navigation: {
                                path: '/CashFlowEditor/Scenario/',
                                query: {selectedScenarioId: to.query.selectedScenarioId, simulationName: to.query.simulationName}
                            }
                        }
                    ];

                    // get the window href
                    const href = window.location.href;
                    // check each NavigationTab object to see if it has a matching navigation path with the href
                    const hasChildPath = any(
                        (navigationTab: NavigationTab) => href.indexOf(navigationTab.navigation.path) !== -1,
                        vm.navigationTabs
                    );
                    // if no matching navigation path was found in the href, then route with path of first navigationTabs entry
                    if (!hasChildPath) {
                        vm.$router.push(vm.navigationTabs[0].navigation);
                    }
                }
            });
        }

        beforeDestroy() {
            this.setSelectedScenarioNameAction({selectedScenarioName: ''});
        }

        /**
         * Shows the CommittedProjectsFileUploaderDialog
         */
        onShowCommittedProjectsFileUploader() {
            this.showFileUploader = true;
        }

        /**
         * Uploads the files submitted via the CommittedProjectsFileUploaderDialog (if present),
         * exports committed projects if isExport is true
         * @param result CommmittedProjectsDialogResult object
         * @param isExport boolean
         */
        onUploadCommittedProjectFiles(result: CommittedProjectsDialogResult, isExport: boolean) {
            this.showFileUploader = false;
            if (!isNil(result)) {
                CommittedProjectsService
                    .saveCommittedProjectsFiles(result.files, result.applyNoTreatment, this.selectedScenarioId.toString(), this.networks[0].networkId.toString())
                    .then((response: AxiosResponse<any>) => {
                        this.setSuccessMessageAction({ message: 'Successfully saved file(s)' });
                    });
            }
            if (isExport) {
                this.selectedScenario.simulationId = this.selectedScenarioId;
                this.selectedScenario.networkId = this.networks[0].networkId;
                CommittedProjectsService.ExportCommittedProjects(this.selectedScenario)
                    .then((response: AxiosResponse<any>) => {
                        FileDownload(response.data, 'CommittedProjects.xlsx');
                    });
            }
        }
    }
</script>

<style>
    .child-router-div {
        height: 100%;
        overflow: auto;
    }
</style>