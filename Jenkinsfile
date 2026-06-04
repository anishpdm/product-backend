pipeline {
    agent any

    environment {
        IMAGE        = "product-api:${BUILD_NUMBER}"
        NETWORK      = "app-net"
        MYSQL_CONT   = "app-mysql"
        API_CONT     = "product-api"
        MYSQL_PWD    = "rootpassword"
        MYSQL_DB     = "productdb"
    }

    stages {
        stage('Checkout') {
            steps { checkout scm }
        }

        stage('Build Docker Image') {
            steps {
                sh 'docker build -t ${IMAGE} .'
            }
        }

        stage('Start MySQL') {
            steps {
                sh 'docker network create ${NETWORK} || true'
                // Start MySQL only if it is not already running
                sh '''
                    if [ -z "$(docker ps -q -f name=^/${MYSQL_CONT}$)" ]; then
                        docker rm -f ${MYSQL_CONT} || true
                        docker run -d --name ${MYSQL_CONT} --network ${NETWORK} \
                          -e MYSQL_ROOT_PASSWORD=${MYSQL_PWD} \
                          -e MYSQL_DATABASE=${MYSQL_DB} \
                          -p 3306:3306 \
                          mysql:8.0
                        echo "Waiting for MySQL to initialise..."
                        sleep 30
                    else
                        echo "MySQL already running."
                    fi
                '''
            }
        }

        stage('Run API') {
            steps {
                sh 'docker rm -f ${API_CONT} || true'
                sh '''
                    docker run -d --name ${API_CONT} --network ${NETWORK} \
                      -e ConnectionStrings__DefaultConnection="Server=${MYSQL_CONT};Port=3306;Database=${MYSQL_DB};User=root;Password=${MYSQL_PWD};" \
                      -p 5000:8080 \
                      ${IMAGE}
                '''
            }
        }
    }
}
