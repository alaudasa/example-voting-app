FROM node
RUN npm install -g phantomjs
ADD . /app
WORKDIR /app
CMD ["/app/test_result.sh"]
