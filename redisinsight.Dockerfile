FROM redis/redisinsight:latest

USER root

RUN apk add --no-cache libsecret shadow

COPY redisinsight-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/redisinsight-entrypoint.sh

ENTRYPOINT ["redisinsight-entrypoint.sh"]

USER node
CMD ["./docker-entry.sh"]
