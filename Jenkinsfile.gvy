pipeline {
    agent {
        node {
            // Let the build run on any node.
            label ''
            // Define a custom workspace, removing all spaces from the path.  Also trims the length to avoid too-long paths
            customWorkspace "${JENKINS_HOME}\\workspace\\${JOB_NAME}".replace('%2F', '_').take(100)
        }
    }
    options {
        // Only keep 10 builds of history
        buildDiscarder(logRotator(numToKeepStr: '10'))
        // Only allow a single instance of this build job (for a given branch) to run at a time
        disableConcurrentBuilds()
    }
 
    stages {
        stage('Prepare') {
            // This stage should contain all your pre-build steps to prep the build environment, restore packages, prep the Sonar scanner, etc.
            steps {
                // This makes the name of the build a bit more like what you may be used to in TFS
                script {
                    currentBuild.displayName = new Date().format( "yyyy.MM.dd" ) + "." + "${BUILD_NUMBER}"
                }
                // Delete the existing build artifacts.  Repeat for any other folders that may get in the way of the new build
                dir('build') { deleteDir() }
                dir('TestResults') { deleteDir() }
 
                // Restore Nuget packages
                dotNetNuGetRestore()
 
                // Begin the Sonar Scanner, using defaults (version: 1.0, language: cs)
                dotNetBeginSonarScanner key: 'DATA:Data.Sentry.Com', name: 'Data.Sentry.Com', version: "${currentBuild.displayName}"
            }
        }
 
        stage('Build') {
            // This stage contains all of the core steps that turn your source code into compiled artifacts.  For .NET, this typically means calls to dotNetBuild,
            // but may also include steps to build web stuff using npm, etc.
            steps {
                // Use defaults (MSBuild_2017, configuration: Release, platform: Any CPU, generateProjectSpecificOutputFolder: false)
                // You can repeat the following line if you have multiple solution files to build
                dotNetBuild 'Sentry.data.sln'
            }
        }
 
        stage('Test') {
            // This stage includes all steps related to running tests against your code.  Typically these are unit tests written using MSTest.
            steps {
                // Use defaults.  You can repeat the following line if you have multiple unit test assemblies to run unit tests from
                dotNetRunTests 'Sentry.data.Web.Tests.dll'
 
                // Gather up the test results and process them so they can be used by Sonar
                dotNetGatherTestResults()
            }
        }
 
        stage('Publish') {
            // This stage includes steps that take the results of this build and archive and publish them for analysis, deployment, etc.
            steps {
                // Send the results to Sonar
                dotNetEndSonarScanner()
 
                // Repeat the next two steps (dotNetArchiveArtifact and recordArtificateToQuartermaster) if you have multiple applications to archive/record
                 
                // This takes everything in a particular subfolder of your build output, zips it up, and archives it into Jenkins. 
                dotNetArchiveArtifact 'Sentry.data.Web.zip', dir: 'Sentry.data.Web\\_PublishedWebsites\\Sentry.data.Web'
				dotNetArchiveArtifact 'Sentry.data.Goldeneye.zip', dir: 'Sentry.data.Goldeneye'
				dotNetArchiveArtifact 'Sentry.data.Bundler.zip', dir: 'Sentry.data.Bundler'
 
                // Record this build into Quartermaster.  The artifact name should match the filename produced in the dotNetArchiveArtifact step above.
                recordArtifactToQuartermaster requiresConfigTransformation: true, appType: 'Web', applicationName: 'DATA', saidAssetKey: 'DATA', artifactName: 'Sentry.data.Web.zip'
				recordArtifactToQuartermaster requiresConfigTransformation: true, appType: 'Service', applicationName: 'GoldenEye', saidAssetKey: 'DATA', artifactName: 'Sentry.data.Goldeneye.zip'
				recordArtifactToQuartermaster requiresConfigTransformation: true, appType: 'Service', applicationName: 'BundleService', saidAssetKey: 'DATA', artifactName: 'Sentry.data.Bundler.zip'
            }
        }
 
    }
}