FROM index.alauda.cn/library/java:openjdk-8-jdk-alpine

RUN mkdir /app

WORKDIR /app

COPY worker-jar-with-dependencies.jar /app/worker-jar-with-dependencies.jar

CMD ["java", "-jar", "/app/worker-jar-with-dependencies.jar"]
